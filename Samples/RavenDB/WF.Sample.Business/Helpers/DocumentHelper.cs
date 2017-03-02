using System;
using System.Collections.Generic;
using System.Linq;
using WF.Sample.Business.Models;
using Raven.Abstractions.Indexing;
using WF.Sample.Business.Workflow;

namespace WF.Sample.Business.Helpers
{

    public class DocumentHelper
    {
        public static List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            using(var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var query = session.Advanced.LuceneQuery<Document>();
                int actual = page * pageSize;
                count = query.QueryResult.TotalResults;

                return session.Advanced
                    .LuceneQuery<Document>()
                    .Skip(actual)
                    .Take(pageSize).ToList();
            }
        }

        public static List<Document> GetInbox(string identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                count = session.Query<WorkflowInbox>().Where(c => c.IdentityId == identityId).Count();

                int actual = page * pageSize;

                var inbox = session
                    .Query<WorkflowInbox>()
                    .Where(c => c.IdentityId == identityId)
                    .Skip(actual)
                    .Take(pageSize);

                var tmp = inbox.ToArray().Select(c => c.ProcessId as ValueType);
                var docs = session.Load<Document>(tmp);
                res.AddRange(docs);
            }
            return res;
        }

        public static List<Document> GetOutbox(string identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {

                if (Workflow.WorkflowInit.Provider.Store.DatabaseCommands.GetIndex("DocumentByWFExecutorId") == null)
                {
                    Workflow.WorkflowInit.Provider.Store.DatabaseCommands.PutIndex("DocumentByWFExecutorId", new IndexDefinition
                    {
                        Map = @"from doc in docs.Documents
                                    from transition in doc.TransitionHistories
                                    select new { WFExecutorId = transition.EmployeeId }"
                    });
                }

                count = session.Advanced.LuceneQuery<Document>("DocumentByWFExecutorId").Where("WFExecutorId:" + identityId).QueryResult.TotalResults;

                int actual = page * pageSize;

                var docs = session.Advanced
                    .LuceneQuery<Document>("DocumentByWFExecutorId")
                    .Where("WFExecutorId:" + identityId)
                    .Skip(actual)
                    .Take(pageSize);

                actual = actual + pageSize;
                res.AddRange(docs);
            }
            return res;
        }

        public static Document Get(Guid id)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                return session.Load<Document>(id);
            }
        }

        public static void Delete(Guid[] ids)
        {
            using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
            {
                var objs = session.Load<Document>(ids.Select(c=>c as ValueType).ToList());
                foreach (Document item in objs)
                {
                    session.Delete(item);
                }

                session.SaveChanges();
            }

            WorkflowInbox[] wis = null;
            do
            {
                foreach (var id in ids)
                {
                    using (var session = Workflow.WorkflowInit.Provider.Store.OpenSession())
                    {
                        wis = session.Query<WorkflowInbox>().Where(c => c.ProcessId == id).ToArray();
                        foreach (var wi in wis)
                            session.Delete<WorkflowInbox>(wi);

                        session.SaveChanges();
                    }
                }
            } while (wis.Length > 0);
        }

        public static DocumentCommandModel[] GetCommands(Guid id, string userId)
        {
            var result = new List<DocumentCommandModel>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, userId);
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new DocumentCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        public static Dictionary<string, string> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }
    }
}
