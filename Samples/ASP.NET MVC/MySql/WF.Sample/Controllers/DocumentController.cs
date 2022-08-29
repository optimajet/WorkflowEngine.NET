using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OptimaJet.Workflow.Core.Model;
using WF.Sample.Business.DataAccess;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private int pageSize = 15;
        public DocumentController(IDocumentRepository documentRepository, IEmployeeRepository employeeRepository)
        {
            _documentRepository = documentRepository;
            _employeeRepository = employeeRepository;
        }

        #region Index
        public ActionResult Index(int page = 1)
        {
            int count = 0;
            return View(new DocumentListModel<DocumentModel>()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _documentRepository.Get(out count, page, pageSize).Select(GetDocumentModel<DocumentModel>).ToList(),
                Count = count,
            });
        }

        public async Task<ActionResult> Inbox(int page = 1)
        {
            var identityId = CurrentUserSettings.GetCurrentUser().ToString();
            
            var inbox = await WorkflowInit.Runtime.PersistenceProvider
                .GetInboxByIdentityIdAsync(identityId, Paging.Create(page, pageSize));
            
            int count = await WorkflowInit.Runtime.PersistenceProvider.GetInboxCountByIdentityIdAsync(identityId);

            return View("Inbox", new DocumentListModel<InboxDocumentModel>()
            {
                Page = page,
                PageSize = pageSize,
                Docs = GetDocumentsByInbox(inbox),
                Count = count,
            });
        }

        public async Task<ActionResult> Outbox(int page = 1)
        {
            var identityId = CurrentUserSettings.GetCurrentUser().ToString();
            
            var outbox = await WorkflowInit.Runtime.PersistenceProvider
                .GetOutboxByIdentityIdAsync(identityId, Paging.Create(page, pageSize));
            
            int count = await WorkflowInit.Runtime.PersistenceProvider.GetOutboxCountByIdentityIdAsync(identityId);
            
            return View("Outbox", new DocumentListModel<OutboxDocumentModel>()
            {
                Page = page,
                Docs =  GetDocumentsByOutbox(outbox),
                PageSize = pageSize,
                Count = count,
            });
        }
        
        #endregion

        #region Edit
        public async Task<ActionResult> Edit(Guid? Id)
        {
            DocumentModel model = null;

            if(Id.HasValue)
            {
                var d = _documentRepository.Get(Id.Value);
                if(d != null)
                {
                    await CreateWorkflowIfNotExists(Id.Value);
                    var history = await GetApprovalHistory(Id.Value);
                    
                    model = new DocumentModel()
                                {
                                    Id = d.Id,
                                    AuthorId = d.AuthorId,
                                    AuthorName = d.Author.Name,
                                    Comment = d.Comment,
                                    ManagerId = d.ManagerId,
                                    ManagerName =
                                        d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                                    Name = d.Name,
                                    Number = d.Number,
                                    StateName = d.StateName,
                                    Sum = d.Sum,
                                    Commands = await GetCommands(Id.Value),
                                    AvailiableStates = await GetStates(Id.Value),
                                    HistoryModel = new DocumentHistoryModel{Items = history}
                                };
                }
                
            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser();
                model = new DocumentModel()
                        {
                            AuthorId = userId,
                            AuthorName = _employeeRepository.GetNameById(userId),
                            StateName = "Vacation request created"
                        };
            }

            return View(model);
        }
        
        public async Task<ActionResult> ExecuteCommand(Guid Id, DocumentModel model, string command)
        {
            await ExecuteCommand(Id, command, model);
            return RedirectToAction("Inbox");
            
        }
        
        [HttpPost]
        public async Task<ActionResult> Edit(Guid? Id, DocumentModel model, string button)
        {
         
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Document doc = Mapper.Map<Document>(model);

            try
            {
                doc = _documentRepository.InsertOrUpdate(doc);

                if (doc == null)
                {
                    ModelState.AddModelError("", "Row not found!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder("Save error. " + ex.Message);
                if (ex.InnerException != null)
                    sb.AppendLine(ex.InnerException.Message);
                ModelState.AddModelError("", sb.ToString());
                return View(model);
            }

            if (button == "SaveAndExit")
                return RedirectToAction("Index");
            if (button != "Save")
            {
                await ExecuteCommand(doc.Id, button, model);
            }
            return RedirectToAction("Edit", new { doc.Id});
            
        }

        #endregion

        #region Delete
        
        public async Task<ActionResult> DeleteRows(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                foreach (var id in ids)
                {
                    await WorkflowInit.Runtime.PersistenceProvider.DeleteProcessAsync(id);
                }
                
                _documentRepository.Delete(ids);
                
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }
        #endregion

        #region Workflow
        private async Task<DocumentCommandModel[]> GetCommands(Guid id)
        {
            var result = new List<DocumentCommandModel>();
            
            var commands = await WorkflowInit.Runtime.GetAvailableCommandsAsync(id, CurrentUserSettings.GetCurrentUser().ToString());
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new DocumentCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        private async Task<Dictionary<string, string>> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = await WorkflowInit.Runtime.GetAvailableStateToSetAsync(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }

        private async Task ExecuteCommand(Guid id, string commandName, DocumentModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser().ToString();
            
            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;
                
                var setStateParams = new SetStateParams(id,document.StateNameToSet)
                {
                    IdentityId = currentUser,
                    ImpersonatedIdentityId = currentUser
                }.AddTemporaryParameter("Comment",document.Comment);
                
                await WorkflowInit.Runtime.SetStateAsync(setStateParams);
              
                return;
            }

            var commands = await WorkflowInit.Runtime.GetAvailableCommandsAsync(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));
            
            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            await WorkflowInit.Runtime.ExecuteCommandAsync(command,currentUser,currentUser);
        }

        private async Task CreateWorkflowIfNotExists(Guid id)
        {
            if (await WorkflowInit.Runtime.IsProcessExistsAsync(id))
                return;

            await WorkflowInit.Runtime.CreateInstanceAsync("SimpleWF", id);
        }

        #endregion

        private TDoc GetDocumentModel<TDoc>(Document d)
            where TDoc:DocumentModel, new()
        {
            return new TDoc()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Author.Name,
                Comment = d.Comment,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }

        private List<InboxDocumentModel> GetDocumentsByInbox(List<InboxItem> inbox)
        {
            var ids = inbox.Select(x => x.ProcessId).Distinct().ToList();
            
            var documents = _documentRepository.GetByIds(ids)
                .ToDictionary(x=>x.Id, x=>x);
            
            var docs = new List<InboxDocumentModel>();
            
            foreach (var inboxItem in inbox)
            {
                InboxDocumentModel doc;
                
                //if document is exists
                if (documents.TryGetValue(inboxItem.ProcessId, out Document _doc))
                {
                    doc = GetDocumentModel<InboxDocumentModel>(_doc);
                }
                else
                {
                    doc = new InboxDocumentModel();
                    doc.Id = inboxItem.ProcessId;
                    doc.IsCorrect = false;
                    doc.StateName = DocumentModel.NotFoundError;
                }

                doc.AvailableCommands = inboxItem.AvailableCommands;
                doc.AddingDate = inboxItem.AddingDate.ToString();
                docs.Add(doc);
            }

            return docs;
        }
        
        private List<OutboxDocumentModel> GetDocumentsByOutbox(List<OutboxItem> outbox)
        {
            var ids = outbox.Select(x => x.ProcessId).Distinct().ToList();
            
            var documents = _documentRepository.GetByIds(ids)
                .ToDictionary(x=>x.Id, x=>x);
            
            var docs = new List<OutboxDocumentModel>();
            
            foreach (var outboxItem in outbox)
            {
                OutboxDocumentModel doc;
                
                //if document is exists
                if (documents.TryGetValue(outboxItem.ProcessId, out Document _doc))
                {
                    doc = GetDocumentModel<OutboxDocumentModel>(_doc);
                }
                else
                {
                    doc = new OutboxDocumentModel();
                    doc.Id = outboxItem.ProcessId;
                    doc.IsCorrect = false;
                    doc.StateName = DocumentModel.NotFoundError;
                }

                doc.ApprovalCount = outboxItem.ApprovalCount;
                doc.FirstApprovalTime = outboxItem.FirstApprovalTime;
                doc.LastApprovalTime = outboxItem.LastApprovalTime;
                doc.LastApproval = outboxItem.LastApproval;
                docs.Add(doc);
            }

            return docs;
        }
        
        private async Task<List<DocumentApprovalHistory>> GetApprovalHistory(Guid id)
        {
            var approvalHistory = await WorkflowInit.Runtime.PersistenceProvider
                .GetApprovalHistoryByProcessIdAsync(id);
            var employees =  _employeeRepository.GetAll();
            List<DocumentApprovalHistory> histories = new List<DocumentApprovalHistory>();
            foreach (var item in approvalHistory)
            {
                Employee employee = null;
                if (Guid.TryParse(item.IdentityId,  out id) )
                {
                    employee = employees.FirstOrDefault(x => x.Id == id);
                }

                var allowedTo = new List<string>();
                foreach (var user in item.AllowedTo)
                {
                    //Get name if it's Guid
                    if(Guid.TryParse(user, out Guid guid))
                    {
                        allowedTo.Add(employees.FirstOrDefault(x => x.Id == guid)?.Name??user);
                    }
                    else
                    {
                        allowedTo.Add(user);
                    }
                }
                
                histories.Add(new DocumentApprovalHistory()
                {
                    Id = item.Id,
                    ProcessId = item.ProcessId,
                    IdentityId  = item.IdentityId,
                    AllowedTo  = string.Join(",", allowedTo),
                    TransitionTime  = item.TransitionTime,
                    Sort  = item.Sort,
                    InitialState  = item.InitialState,
                    DestinationState  = item.DestinationState,
                    TriggerName  = item.TriggerName,
                    Commentary  = item.Commentary,
                    Employee = employee
                });
            }

            return histories;
        }
    }
}
