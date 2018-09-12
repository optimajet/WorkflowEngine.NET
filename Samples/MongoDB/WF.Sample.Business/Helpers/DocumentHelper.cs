using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WF.Sample.Business.Models;
using WF.Sample.Business.Properties;
using OptimaJet.Workflow.MongoDB;
using WF.Sample.Business.Workflow;
using MongoDB.Driver;

namespace WF.Sample.Business.Helpers
{

    public class DocumentHelper
    {
        public static List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            var query = dbcoll.AsQueryable();
            int actual = page * pageSize;
            count = query.Count();

            return query.Skip(actual).Take(pageSize).ToList();
        }

        public static List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();

            var dbcollInbox = WorkflowInit.Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");

            count = (int)dbcollInbox.CountDocuments(c => c.IdentityId == identityId.ToString("N"));
            int actual = page * pageSize;

            var inbox = dbcollInbox.Find(c => c.IdentityId == identityId.ToString("N")).Skip(actual).Limit(pageSize).ToList();

            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            var docs = dbcoll.Find(Builders<Document>.Filter.In(c => c.Id, inbox.Select(i => i.ProcessId))).ToList();
            res.AddRange(docs);
            return res;
        }

        public static List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
          
            int actual = page * pageSize;
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            var query = dbcoll.AsQueryable().Where(c => c.TransitionHistories.Any(t=>t.EmployeeId == identityId));
            count = query.Count();

            var docs = query.Skip(actual).Take(pageSize).ToArray();
                res.AddRange(docs);

            return res;
        }

        public static Document Get(Guid id)
        {
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");
            return dbcoll.Find(x => x.Id == id).FirstOrDefault();
        }

        public static void Delete(Guid[] ids)
        {
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Document>("Document");

            dbcoll.DeleteMany(Builders<Document>.Filter.In(c => c.Id, ids));

            var dbcollInbox = WorkflowInit.Provider.Store.GetCollection<WorkflowInbox>("WorkflowInbox");
            dbcollInbox.DeleteMany(Builders<WorkflowInbox>.Filter.In(c => c.ProcessId, ids));
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
