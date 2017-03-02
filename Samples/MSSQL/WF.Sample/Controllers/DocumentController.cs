using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business;
using WF.Sample.Business.Helpers;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;
using ProcessStatus = OptimaJet.Workflow.Core.Persistence.ProcessStatus;
using System.Threading;

namespace WF.Sample.Controllers
{
    public class DocumentController : Controller
    {
        #region Index
        public ActionResult Index(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;
            return View(new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = DocumentHelper.Get(out count, page, pageSize)
                        .Select(c=> GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult Inbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;
            return View("Index", new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = DocumentHelper.GetInbox(CurrentUserSettings.GetCurrentUser(), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult Outbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;
            return View("Index", new DocumentListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = DocumentHelper.GetOutbox(CurrentUserSettings.GetCurrentUser(), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? Id)
        {
            DocumentModel model = null;

            if(Id.HasValue)
            {
                var d = DocumentHelper.Get(Id.Value);
                if(d != null)
                {
                    CreateWorkflowIfNotExists(Id.Value);
                    var h = DocumentHelper.GetHistory(Id.Value);
                    model = new DocumentModel()
                                {
                                    Id = d.Id,
                                    AuthorId = d.AuthorId,
                                    AuthorName = d.Employee1.Name,
                                    Comment = d.Comment,
                                    EmloyeeControlerId = d.EmloyeeControlerId,
                                    EmloyeeControlerName =
                                        d.EmloyeeControlerId.HasValue ? d.Employee.Name : string.Empty,
                                    Name = d.Name,
                                    Number = d.Number,
                                    StateName = d.StateName,
                                    Sum = d.Sum,
                                    Commands = GetCommands(Id.Value),
                                    AvailiableStates = GetStates(Id.Value),
                                    HistoryModel = new DocumentHistoryModel{Items = h}
                                };
                }
                
            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser();
                model = new DocumentModel()
                        {
                            AuthorId = userId,
                            AuthorName = EmployeeHelper.GetNameById(userId),
                            StateName = "Draft"
                        };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Guid? Id, DocumentModel model, string button)
        {
            using (var context = new DataModelDataContext())
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Document target = null;
                if (model.Id != Guid.Empty)
                {
                    target = context.Documents.SingleOrDefault(d=>d.Id == model.Id);
                    if (target == null)
                    {
                        ModelState.AddModelError("", "Row not found!");
                        return View(model);
                    }
                }
                else
                {
                    target = new Document();
                    target.Id = Guid.NewGuid();
                    target.AuthorId = model.AuthorId;
                    target.StateName = model.StateName;
                    context.Documents.InsertOnSubmit(target);
                }

                target.Name = model.Name;
                target.EmloyeeControlerId = model.EmloyeeControlerId;
                target.Comment = model.Comment;
                target.Sum = model.Sum;

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    var sb = new StringBuilder("Ошибка сохранения. " + ex.Message);
                    if (ex.InnerException != null)
                        sb.AppendLine(ex.InnerException.Message);
                    ModelState.AddModelError("", sb.ToString());
                    return View(model);
                }

                if (button == "SaveAndExit")
                    return RedirectToAction("Index");
                if (button != "Save")
                {
                    ExecuteCommand(target.Id, button, model);
                }
                return RedirectToAction("Edit", new {target.Id});
            }
        }
        #endregion

        #region Delete
        public ActionResult DeleteRows(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                DocumentHelper.Delete(ids);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }
        #endregion

        #region Workflow
        private DocumentCommandModel[] GetCommands(Guid id)
        {
            var result = new List<DocumentCommandModel>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, CurrentUserSettings.GetCurrentUser().ToString("N"));
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new DocumentCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        private Dictionary<string, string> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }

        private void ExecuteCommand(Guid id, string commandName, DocumentModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser().ToString("N");
            
            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(id, currentUser, currentUser, document.StateNameToSet, new Dictionary<string, object> { { "Comment", document.Comment } });
                return;
            }

            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));
            
            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(command,currentUser,currentUser);
        }

        private void CreateWorkflowIfNotExists(Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            WorkflowInit.Runtime.CreateInstance("SimpleWF", id);
        }

        #endregion

        public ActionResult RecalcInbox()
        {
            var newThread = new Thread(WorkflowInit.RecalcInbox);
            newThread.Start();
            return Content("Calculating inbox started!");
        }

        private DocumentModel GetDocumentModel(Document d)
        {
            return new DocumentModel()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Employee1.Name,
                Comment = d.Comment,
                EmloyeeControlerId = d.EmloyeeControlerId,
                EmloyeeControlerName = d.EmloyeeControlerId.HasValue ? d.Employee.Name : string.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }
    }
}
