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
using WF.Sample.Business.Models;

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
                Docs = DocumentHelper.Get(out count, page, pageSize),
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
                Docs = DocumentHelper.GetInbox(CurrentUserSettings.GetCurrentUser(), out count, page, pageSize),
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
                Docs = DocumentHelper.GetOutbox(CurrentUserSettings.GetCurrentUser(), out count, page, pageSize),
                Count = count,
            });
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? Id)
        {
            Document model = null;

            if (Id.HasValue)
            {
                CreateWorkflowIfNotExists(Id.Value);
                model = DocumentHelper.Get(Id.Value);
            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser();
                model = new Document()
                {
                    AuthorId = userId,
                    AuthorName = EmployeeHelper.GetNameById(userId),
                    StateName = "Draft"
                };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Guid? Id, Document model, string button)
        {
            Document target = null;

            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                if (model.Id != Guid.Empty)
                {
                    target = dbcoll.FindOneById(model.Id);
                    if (target == null)
                    {
                        ModelState.AddModelError("", "Row not found!");
                        return View(model);
                    }

                    target.Name = model.Name;
                    target.EmloyeeControlerId = model.EmloyeeControlerId;
                    target.EmloyeeControlerName = model.EmloyeeControlerName;
                    target.Comment = model.Comment;
                    target.Sum = model.Sum;
                    dbcoll.Save(target);
                }
                else
                {
                    target = new Document();
                    target.Id = Guid.NewGuid();
                    target.AuthorId = model.AuthorId;
                    target.AuthorName = model.AuthorName;
                    target.StateName = model.StateName;
                    target.Number = GetNextNumber();
                    target.Name = model.Name;
                    target.EmloyeeControlerId = model.EmloyeeControlerId;
                    target.EmloyeeControlerName = model.EmloyeeControlerName;
                    target.Comment = model.Comment;
                    target.Sum = model.Sum;
                    dbcoll.Insert(target);
                }
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
            return RedirectToAction("Edit", new { target.Id });
        }

        private int GetNextNumber()
        {
            int res = 1;
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<SettingParam<int>>("SettingParam");
            var number = dbcoll.FindOneById("documentnumber");
            if (number == null)
            {
                dbcoll.Insert(new SettingParam<int> { Id = "documentnumber", Value = res + 1 });
            }
            else
            {
                res = number.Value;
                number.Value += 1;
                dbcoll.Save(number);
            }
            return res;
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
                WorkflowInit.Provider.DeleteProcess(ids);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }
        #endregion

        #region Workflow


        private void ExecuteCommand(Guid id, string commandName, Document document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser().ToString("N");

            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(id, currentUser, currentUser, document.StateNameToSet, new Dictionary<string, object> { { "Comment", document.Comment } });
                return;
            }

            if (WorkflowInit.Runtime.GetCurrentStateName(id) == "Draft")
                WorkflowInit.Runtime.PreExecuteFromInitialActivity(id);


            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));

            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(command, currentUser, currentUser);
        }

        private void CreateWorkflowIfNotExists(Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            using (var sync = new WorkflowSync(WorkflowInit.Runtime, id))
            {
                WorkflowInit.Runtime.CreateInstance("SimpleWF", id);

                sync.StatrtWaitingFor(new List<ProcessStatus> { ProcessStatus.Idled, ProcessStatus.Initialized });

                sync.Wait(new TimeSpan(0, 0, 10));
            }
        }
        #endregion

        public ActionResult RecalcInbox()
        {
            //var newThread = new Thread(WorkflowInit.RecalcInbox);
            //newThread.Start();
            return Content("NotImplementedException!");
        }

        private Document GetDocumentModel(Document d)
        {
            return new Document()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.AuthorName,
                Comment = d.Comment,
                EmloyeeControlerId = d.EmloyeeControlerId,
                EmloyeeControlerName = d.EmloyeeControlerName,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }
    }
}
