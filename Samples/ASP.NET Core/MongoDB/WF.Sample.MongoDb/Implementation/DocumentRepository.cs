using MongoDB.Driver;
using OptimaJet.Workflow.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.MongoDb.Helpers;

namespace WF.Sample.MongoDb.Implementation
{
    public class DocumentRepository : IDocumentRepository
    {
        private static IMongoDatabase Store => (WorkflowInit.Runtime.PersistenceProvider as MongoDBProvider).Store;

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            var docdbcoll = Store.GetCollection<Entities.Document>("Document");
            var document = docdbcoll.Find(x => x.Id == id).FirstOrDefault();
            if (document != null)
            {
                document.State = nextState;
                document.StateName = nextStateName;
                docdbcoll.ReplaceOne(x => x.Id == document.Id, document, new UpdateOptions {IsUpsert = true});
            }
        }

        public void Delete(Guid[] ids)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");

            dbcoll.DeleteMany(Builders<Entities.Document>.Filter.In(c => c.Id, ids));

            var dbcollInbox = Store.GetCollection<Entities.WorkflowInbox>("WorkflowInbox");
            dbcollInbox.DeleteMany(Builders<Entities.WorkflowInbox>.Filter.In(c => c.ProcessId, ids));
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var doc = dbcoll.Find(x => x.Id == processId).FirstOrDefault();
            if (doc != null)
            {
                doc.TransitionHistories.RemoveAll(dth => !dth.TransitionTime.HasValue);
                dbcoll.ReplaceOne(x => x.Id == doc.Id, doc, new UpdateOptions { IsUpsert = true });
            }
        }

        public List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");

            var query = dbcoll.AsQueryable();
            int actual = page * pageSize;
            count = query.Count();

            return query.Skip(actual).Take(pageSize).ToList().Select(x => Mappings.Mapper.Map<Document>(x)).ToList();
        }

        public Document Get(Guid id, bool loadChildEntities = true)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var doc = dbcoll.Find(x => x.Id == id).FirstOrDefault();
            if (doc == null) return null;

            return Mappings.Mapper.Map<Document>(doc);
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            var res = new List<string>();

            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var document = dbcoll.Find(x => x.Id == documentId).FirstOrDefault();
            if (document == null)
                return res;

            var sds = CacheHelper<StructDivision>.Cache;
            var emps = CacheHelper<Entities.Employee>.Cache;

            var author = emps.FirstOrDefault(c => c.Id == document.AuthorId);
            if (author == null)
                return res;

            var currentSD = sds.FirstOrDefault(c => c.Id == author.StructDivisionId);
            while (currentSD != null)
            {
                var headEmpIds = emps.Where(c => c.IsHead && c.StructDivisionId == currentSD.Id).Select(c => c.Id.ToString()).ToArray();
                res.AddRange(headEmpIds);

                if (currentSD.ParentId != null)
                    currentSD = sds.FirstOrDefault(c => c.Id == currentSD.ParentId);
                else
                    currentSD = null;
            }

            return res.Distinct();
        }

        public List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var doc = dbcoll.Find(x => x.Id == id).FirstOrDefault();
            if (doc == null) return null;

            return doc.TransitionHistories.Select(h => Mappings.Mapper.Map<DocumentTransitionHistory>(h)).ToList();
        }

        public List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();

            var dbcollInbox = Store.GetCollection<Entities.WorkflowInbox>("WorkflowInbox");

            count = (int)dbcollInbox.CountDocuments(c => c.IdentityId == identityId.ToString());
            int actual = page * pageSize;

            var inbox = dbcollInbox.Find(c => c.IdentityId == identityId.ToString()).Skip(actual).Limit(pageSize).ToList();

            var dbcoll = Store.GetCollection<Entities.Document>("Document");

            var docs = dbcoll.Find(Builders<Entities.Document>.Filter.In(c => c.Id, inbox.Select(i => i.ProcessId))).ToList();

            res.AddRange(docs.Select(d => Mappings.Mapper.Map<Document>(d)));
            return res;
        }

        public List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();

            int actual = page * pageSize;
            var dbcoll = Store.GetCollection<Entities.Document>("Document");

            var query = dbcoll.Find(Builders<Entities.Document>.Filter.ElemMatch(x => x.TransitionHistories, x => x.EmployeeId == identityId));

            count = (int)query.CountDocuments();

            var docs = query.Skip(actual).Limit(pageSize).ToList();
            res.AddRange(docs.Select(d => Mappings.Mapper.Map<Document>(d)));

            return res;
        }

        public Document InsertOrUpdate(Document doc)
        {
            Entities.Document target = null;

            var dbcoll = Store.GetCollection<Entities.Document>("Document");

            if (doc.Id != Guid.Empty)
            {
                target = dbcoll.Find(x => x.Id == doc.Id).FirstOrDefault();
                if (target == null)
                {
                    return null;
                }

                target.Name = doc.Name;
                target.ManagerId = doc.ManagerId;
                target.ManagerName = doc.Manager?.Name;
                target.Comment = doc.Comment;
                target.Sum = doc.Sum;
                dbcoll.ReplaceOne(x => x.Id == target.Id, target, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                target = new Entities.Document();
                target.Id = Guid.NewGuid();
                target.AuthorId = doc.AuthorId;
                target.AuthorName = doc.Author.Name;
                target.StateName = doc.StateName;
                target.Number = GetNextNumber();
                target.Name = doc.Name;
                target.ManagerId = doc.ManagerId;
                target.ManagerName = doc.Manager?.Name;
                target.Comment = doc.Comment;
                target.Sum = doc.Sum;
                dbcoll.InsertOne(target);

                doc.Number = target.Number;
                doc.Id = target.Id;
            }

            return doc;
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            return GetAuthorsBoss(documentId).Contains(identityId.ToString());
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var doc = dbcoll.Find(x => x.Id == id).FirstOrDefault();

            if (doc == null) throw new ArgumentException("Document not found", "id");

            var historyItem = doc.TransitionHistories.FirstOrDefault(h => !h.TransitionTime.HasValue && 
                h.InitialState == currentState && 
                h.DestinationState == nextState);

            if (historyItem == null)
            {
                historyItem = new Entities.DocumentTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = string.Empty,
                    DestinationState = nextState,
                    InitialState = currentState
                };

                doc.TransitionHistories.Add(historyItem);
            }

            historyItem.Command = command;
            historyItem.TransitionTime = DateTime.Now;
            historyItem.EmployeeId = employeeId;
            
            if(employeeId.HasValue)
            {
                var employee = Store.GetCollection<Entities.Employee>("Employee").Find(x => x.Id == employeeId.Value).FirstOrDefault();
                historyItem.EmployeeName = employee.Name;
            }else
            {
                historyItem.EmployeeName = null;
            }

            dbcoll.ReplaceOne(x => x.Id == doc.Id, doc, new UpdateOptions { IsUpsert = true });
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            var dbcoll = Store.GetCollection<Entities.Document>("Document");
            var doc = dbcoll.Find(x => x.Id == id).FirstOrDefault();

            if (doc == null) throw new ArgumentException("Document not found", "id");

            var historyItem = new Entities.DocumentTransitionHistory
            {
                Id = Guid.NewGuid(),
                AllowedToEmployeeNames = GetEmployeesString(identities),
                DestinationState = nextState,
                InitialState = currentState,
                Command = command
            };

            doc.TransitionHistories.Add(historyItem);
            dbcoll.ReplaceOne(x => x.Id == doc.Id, doc, new UpdateOptions { IsUpsert = true });
        }

        private string GetEmployeesString(IEnumerable<string> identities)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = CacheHelper<Entities.Employee>.Cache.Where(e => identitiesGuid.Contains(e.Id)).ToList();

            var sb = new StringBuilder();
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                    sb.Append(",");
                isFirst = false;

                sb.Append(employee.Name);
            }

            return sb.ToString();
        }

        private int GetNextNumber()
        {
            int res = 1;
            var dbcoll = Store.GetCollection<Entities.SettingParam<int>>("SettingParam");
            var number = dbcoll.Find(x => x.Id == "documentnumber").FirstOrDefault();
            if (number == null)
            {
                dbcoll.InsertOne(new Entities.SettingParam<int> { Id = "documentnumber", Value = res + 1 });
            }
            else
            {
                res = number.Value;
                number.Value += 1;
                dbcoll.ReplaceOne(x => x.Id == number.Id, number, new UpdateOptions { IsUpsert = true });
            }
            return res;
        }
    }
}
