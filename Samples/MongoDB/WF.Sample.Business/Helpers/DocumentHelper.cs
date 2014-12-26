using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WF.Sample.Business.Models;
using WF.Sample.Business.Properties;
using OptimaJet.Workflow.MongoDB;
using MongoDB.Driver.Builders;
using WF.Sample.Business.Workflow;

namespace WF.Sample.Business.Helpers
{

    public class DocumentHelper
    {
        public static List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            var query = dbcoll.FindAll();
            int actual = page * pageSize;
            count = (int)query.Count();

            return query.Skip(actual).Take(pageSize).ToList();
        }

        public static List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();

            var dbcollInbox = Workflow.WorkflowInit.Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");

            count = (int)dbcollInbox.Find(Query<WorkflowInbox>.Where(c => c.IdentityId == identityId.ToString("N"))).Count();
            int actual = page * pageSize;

            var inbox = dbcollInbox.Find(Query<WorkflowInbox>.Where(c => c.IdentityId == identityId.ToString("N"))).Skip(actual).Take(pageSize).ToArray();

            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            var docs = dbcoll.Find(Query<Document>.In(c=>c.Id, inbox.Select(i=>i.ProcessId)));
            res.AddRange(docs);
            return res;
        }

        public static List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
          
            int actual = page * pageSize;
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            var query = dbcoll.Find(Query<Document>.Where(c => c.TransitionHistories.Any(t=>t.EmployeeId == identityId)));
            count = (int)query.Count();

            var docs = query.Skip(actual).Take(pageSize).ToArray();
                res.AddRange(docs);

            return res;
        }

        public static Document Get(Guid id)
        {
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            return dbcoll.FindOneById(id);
        }

        public static void Delete(Guid[] ids)
        {
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            dbcoll.Remove(Query<Document>.In(c => c.Id, ids));

            var dbcollInbox = Workflow.WorkflowInit.Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");
            dbcollInbox.Remove(Query<WorkflowInbox>.In(c => c.ProcessId, ids));
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
