using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business.Helpers;
using WF.Sample.Business;
using WF.Sample.Business.Workflow;
using WF.Sample.Models;

namespace WF.Sample.Controllers
{
    public class LoadTestingController : Controller
    {
        public ActionResult Index(int? GraphUnit)
        {
            var res = GetStatistics(GraphUnit ?? 60);
            return View(res);
        }

        public ActionResult Run(int doccount, int threadcount, int wfcommandcount, int wfthreadcount)
        {
            for (int i = 0; i < threadcount; i++)
            {
                Thread myThread = new Thread(DocCreate);
                myThread.Start(doccount);
            }

            Thread.Sleep(1000);

            for (int i = 0; i < wfthreadcount; i++)
            {
                Thread myThread = new Thread(WFCommandExecute);
                myThread.Start(wfcommandcount);
            }

            return Content("Starting success!");
        }

        public ActionResult Clean()
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadTestingOperations.DeleteAllOnSubmit(context.LoadTestingOperations);
                context.SubmitChanges();
            }

            return Content("");
        }

        #region DocCreate
        private void DocCreate(object cnt)
        {
            var emps = EmployeeHelper.GetAll();
            Random r = new Random(Environment.TickCount);

            int count = (int)cnt;
            for (int i = 0; i < count; i++)
            {
                Guid id = CreateDocument(emps, r);
                DocumentCreateWorkflow(id);
            }
        }

        private void DocumentCreateWorkflow(Guid id)
        {
            DateTime opStart = DateTime.Now;
            WorkflowInit.Runtime.CreateInstance("SimpleWF", id);
            AddOperation(opStart, DateTime.Now, "CreatingWorkflow");
        }

        private Guid CreateDocument(List<Employee> emps, Random r)
        {
            Guid id = Guid.NewGuid();

            DateTime opStart = DateTime.Now;
            using (var context = new DataModelDataContext())
            {
                var author = emps[r.Next(0, emps.Count)];
                var controller = emps[r.Next(0, emps.Count)];

                var doc = new Document();
                doc.Id = id;
                doc.AuthorId = author.Id;
                doc.StateName = "Draft";
                doc.Name = "AG_Doc " + doc.Number.ToString();

                if (r.Next(0, 2) == 1)
                {
                    doc.EmloyeeControlerId = controller.Id;
                }
                doc.Comment = string.Format("Auto-generated documnet. {0}.", DateTime.Now.ToShortTimeString());
                doc.Sum = r.Next(1, 10000);

                context.Documents.InsertOnSubmit(doc);
                context.SubmitChanges();
            }

            AddOperation(opStart, DateTime.Now, "CreatingDocument");

            return id;
        }
        #endregion

        #region WF Command

        private void WFCommandExecute(object cnt)
        {
            var emps = EmployeeHelper.GetAll();
            Random r = new Random(Environment.TickCount);

            int count = (int)cnt;
            for (int i = 0; i < count; )
            {
                int oldI = i;
                for (int k = 0; k < emps.Count - 1; k++)
                {
                    var employee = emps[k];
                    Guid? docId = null;
                    using (var context = new DataModelDataContext())
                    {
                        int inboxCount = context.WorkflowInboxes.Count(c => c.IdentityId == employee.Id);
                        if (inboxCount > 0)
                        {
                            var tmp = context.WorkflowInboxes
                                .Where(c => c.IdentityId == employee.Id)
                                .Skip(r.Next(0, inboxCount))
                                .Take(1)
                                .FirstOrDefault();

                            if (tmp != null)
                                docId = tmp.ProcessId;
                        }
                    }

                    if (docId.HasValue)
                    {
                        DateTime opStart = DateTime.Now;
                        var commands = WorkflowInit.Runtime.GetAvailableCommands(docId.Value, employee.Id.ToString("N")).ToArray();
                        AddOperation(opStart, DateTime.Now, "GetAvailableCommands");

                        if (commands.Length > 0)
                        {
                            var c = commands[r.Next(0, commands.Length)];
                            c.SetParameter("Comment", "Load testing. ExecuteCommand");

                            opStart = DateTime.Now;

                            try
                            {
                                WorkflowInit.Runtime.ExecuteCommand(docId.Value, employee.Id.ToString("N"), employee.Id.ToString("N"), c);
                            }
                            catch (ImpossibleToSetStatusException ex)
                            {
                                //If process is Running then ignore it's
                                continue;
                            }
                            catch (CommandNotValidForStateException ex)
                            {
                                //If process is changed state then ignore it's
                                continue;
                            }

                            AddOperation(opStart, DateTime.Now, "ExecuteCommand");
                            i++;
                            break;
                        }
                    }
                }

                if (oldI == i)
                    break;
            }
        }

        #endregion

        #region Statistics
        private void AddOperation(DateTime opStart, DateTime opEnd, string type)
        {
            using (var context = new DataModelDataContext())
            {
                TimeSpan duration = opEnd - opStart;

                context.LoadTestingOperations.InsertOnSubmit(new LoadTestingOperation()
                {
                    Id = Guid.NewGuid(),
                    Date = opStart,
                    Type = type,
                    DurationMilliseconds = duration.TotalMilliseconds
                });
                context.SubmitChanges();
            }
        }

        private List<LoadTestingStatisticsModel> GetStatistics(int unit)
        {
            TimeSpan ts = new TimeSpan(0, 0, unit);

            var res = new List<LoadTestingStatisticsModel>();

            using (var context = new DataModelDataContext())
            {
                foreach (var op in context.LoadTestingOperations)
                {
                    op.Date = Floor(op.Date, ts);
                    var r = res.FirstOrDefault(c => c.Date == op.Date);
                    if (r == null)
                    {
                        r = new LoadTestingStatisticsModel() { Date = op.Date };
                        res.Add(r);
                    }

                    var item = r.Items.FirstOrDefault(c => c.Type == op.Type);
                    if (item == null)
                    {
                        item = new LoadTestingStatisticItemModel()
                        {
                            Type = op.Type
                        };
                        r.Items.Add(item);
                    }

                    item.Duration += op.DurationMilliseconds;
                    item.CheckDurationMinMax(op.DurationMilliseconds);
                    item.Count++;
                }
            }

            return res;
        }
        private DateTime Floor(DateTime date, TimeSpan span)
        {
            long ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }
        #endregion
    }
}
