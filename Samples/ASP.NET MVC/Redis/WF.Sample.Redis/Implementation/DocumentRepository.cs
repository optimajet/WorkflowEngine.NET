using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.Redis.Implementation
{
    public class DocumentRepository : RepositoryBase, IDocumentRepository
    {

        public DocumentRepository(ConnectionSettingsProvider settings) : base(settings)
        {
        }

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            var db = _connector.GetDatabase();
            var doc = GetDocument(db, id);

            if (doc != null)
            {
                doc.StateName = nextStateName;
                doc.State = nextState;
                SaveDocument(db, doc);
            }
        }

        public void Delete(Guid[] ids)
        {
            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();

            var docs = db.StringGet(ids.Select(k => (RedisKey)GetKeyForDocument(k)).ToArray())
                .Select(d => JsonConvert.DeserializeObject<Entities.Document>(d));


            foreach (var doc in docs)
            {
                var employeesKey = GetKeyForInboxEmployees(doc.Id);
                var inboxEmployees = db.SetMembers(employeesKey);

                foreach(var employeeIndex in inboxEmployees)
                {
                    if(employeeIndex.HasValue)
                    {
                        var guid = new Guid(employeeIndex.ToString());
                        batch.ListRemoveAsync(GetKeyForInboxDocuments(guid), doc.Id.ToString());
                    }
                }

                foreach(var h in doc.TransitionHistories)
                {
                    if(h.EmployeeId.HasValue)
                    {
                        batch.SortedSetRemoveAsync(GetKeyForOutboxDocuments(h.EmployeeId.Value), doc.Id.ToString());
                        Debug.Write($"{GetKeyForOutboxDocuments(h.EmployeeId.Value)} removed from outbox");
                    }
                }

                batch.KeyDeleteAsync(employeesKey);
            }

            batch.KeyDeleteAsync(ids.Select(k => (RedisKey)GetKeyForDocument(k)).ToArray());

            batch.KeyDeleteAsync(ids.Select(k => (RedisKey)GetKeyForInboxEmployees(k)).ToArray());

            batch.SortedSetRemoveAsync(GetKeyForSortedDocuments(), ids.Select(i => (RedisValue)i.ToString()).ToArray());

            batch.Execute();
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            var db = _connector.GetDatabase();
            var doc = GetDocument(db, processId);
            
            if (doc != null)
            {
                var employeeIdsOfEmptyPreHistory = doc.TransitionHistories
                    .Where(x => !x.TransitionTime.HasValue && x.EmployeeId.HasValue)
                    .Select(x => x.EmployeeId.Value).Distinct();

                doc.TransitionHistories.RemoveAll(dth => !dth.TransitionTime.HasValue);

                var employeeIdsToDelete = employeeIdsOfEmptyPreHistory.Where(x => !doc.TransitionHistories.Any(h => h.EmployeeId == x));

                var batch = db.CreateBatch();

                SaveDocument(batch, doc);

                foreach(var id in employeeIdsToDelete)
                {
                    batch.SortedSetRemoveAsync(GetKeyForOutboxDocuments(id), doc.Id.ToString());
                }

                batch.Execute();
            }
        }

        public List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            var db = _connector.GetDatabase();
            var documentCollectionKey = GetKeyForSortedDocuments();

            count = (int)db.SortedSetLength(documentCollectionKey);

            var keys = db.SortedSetRangeByRank(documentCollectionKey, page * pageSize, (page + 1) * pageSize - 1, Order.Descending);

            var docs = db.StringGet(keys.Select(k => (RedisKey)GetKeyForDocument(new Guid((string)k))).ToArray());

            return docs.Select(d => Mappings.Mapper.Map<Document>(JsonConvert.DeserializeObject<Entities.Document>(d))).ToList();
        }

        public Document Get(Guid id, bool loadChildEntities = true)
        {
            var db = _connector.GetDatabase();
            var doc = GetDocument(db, id);

            if (doc == null) return null;

            return Mappings.Mapper.Map<Document>(doc);
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            var res = new List<string>();

            var db = _connector.GetDatabase();
            var doc = GetDocument(db, documentId);

            if (doc == null) return res;

            var author = GetEmployee(db, doc.AuthorId);

            if (author == null)
            {
                return res;
            }

            var divisionValue = db.StringGet(GetKeyForStructDivision(author.StructDivisionId));

            while(divisionValue.HasValue)
            {
                var division = JsonConvert.DeserializeObject<Entities.StructDivision>(divisionValue);

                res.AddRange(division.Heads.Select(x => x.ToString()));

                if (division.ParentId != null)
                    divisionValue = db.StringGet(GetKeyForStructDivision(division.ParentId.Value));
                else
                    break;
            }

            return res.Distinct();
        }

        public List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            var db = _connector.GetDatabase();
            var document = GetDocument(db, id);
            if (document == null)
                return null;

            return document.TransitionHistories.Select(h => Mappings.Mapper.Map<DocumentTransitionHistory>(h)).ToList();
        }

        public List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var db = _connector.GetDatabase();

            var inboxKey = GetKeyForInboxDocuments(identityId);

            count = (int)db.ListLength(inboxKey);

            var inbox = db.ListRange(inboxKey, page * pageSize, (page + 1) * pageSize - 1);

            var docs = db.StringGet(inbox.Select(k => (RedisKey)GetKeyForDocument(new Guid((string)k))).ToArray());

            return docs.Select(d => Mappings.Mapper.Map<Document>(JsonConvert.DeserializeObject<Entities.Document>(d))).ToList();
        }

        public List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();

            var db = _connector.GetDatabase();

            var outboxKey = GetKeyForOutboxDocuments(identityId);

            count = (int)db.SortedSetLength(outboxKey);

            var outbox = db.SortedSetRangeByRank(outboxKey, page * pageSize, (page + 1) * pageSize - 1);

            var docs = db.StringGet(outbox.Select(k => (RedisKey)GetKeyForDocument(new Guid((string)k))).ToArray());

            return docs.Select(d => Mappings.Mapper.Map<Document>(JsonConvert.DeserializeObject<Entities.Document>(d))).ToList();
        }

        public Document InsertOrUpdate(Document doc)
        {
            Entities.Document target = null;

            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();

            if (doc.Id != Guid.Empty)
            {
                target = GetDocument(db, doc.Id);

                if (target == null)
                {
                    return null;
                }
            }
            else
            {
                target = new Entities.Document
                {
                    Id = Guid.NewGuid(),
                    AuthorId = doc.AuthorId,
                    AuthorName = doc.Author.Name,
                    StateName = doc.StateName,
                    State = doc.State,
                    Number = GetNextDocumentNumber(db)
                };

                doc.Number = target.Number;
                doc.Id = target.Id;

                batch.SortedSetAddAsync(GetKeyForSortedDocuments(), target.Id.ToString(), target.Number.Value);
            }

            target.Name = doc.Name;
            target.ManagerId = doc.ManagerId;
            target.ManagerName = doc.Manager?.Name;
            target.Comment = doc.Comment;
            target.Sum = doc.Sum;

            SaveDocument(batch, target);

            batch.Execute();

            return doc;
            
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            return GetAuthorsBoss(documentId).Contains(identityId.ToString());
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            var db = _connector.GetDatabase();

            var doc = GetDocument(db, id);

            if (doc == null) throw new ArgumentException("Document not found", nameof(id));

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

            if (employeeId.HasValue)
            {
                var employee = GetEmployee(db, employeeId.Value);
                historyItem.EmployeeName = employee.Name;
            }
            else
            {
                historyItem.EmployeeName = null;
            }

            var batch = db.CreateBatch();

            SaveDocument(batch, doc);
            if (employeeId.HasValue)
            {
                batch.SortedSetAddAsync(GetKeyForOutboxDocuments(employeeId.Value), id.ToString(), GetNextOutboxNumber(db));
            }

            batch.Execute();
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            var db = _connector.GetDatabase();

            var doc = GetDocument(db, id);

            if (doc == null) throw new ArgumentException("Document not found", "id");

            var historyItem = new Entities.DocumentTransitionHistory
            {
                Id = Guid.NewGuid(),
                AllowedToEmployeeNames = GetEmployeesString(db, identities),
                DestinationState = nextState,
                InitialState = currentState,
                Command = command
            };

            doc.TransitionHistories.Add(historyItem);

            SaveDocument(db, doc);
        }

        private string GetEmployeesString(IDatabase database, IEnumerable<string> identities)
        {
            var employees = database.StringGet(identities.Distinct().Select(k => (RedisKey)GetKeyForEmployee(new Guid(k))).ToArray())
                                    .Select(d => JsonConvert.DeserializeObject<Entities.Employee>(d));

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

        private Entities.Document GetDocument(IDatabase database, Guid id)
        {
            var key = GetKeyForDocument(id);
            var doc = database.StringGet(key);

            if (doc.HasValue)
            {
                return JsonConvert.DeserializeObject<Entities.Document>(doc);
            }

            return null;
        }

        private void SaveDocument(IDatabase database, Entities.Document document)
        {
            database.StringSet(GetKeyForDocument(document.Id), JsonConvert.SerializeObject(document));
        }

        private void SaveDocument(IBatch batch, Entities.Document document)
        {
            batch.StringSetAsync(GetKeyForDocument(document.Id), JsonConvert.SerializeObject(document));
        }

        private int GetNextDocumentNumber(IDatabase database)
        {
            return (int)database.StringIncrement(GetkeyForDocumentNumber()) + 1;
        }

        private long GetNextOutboxNumber(IDatabase database)
        {
            return (int)database.StringIncrement(GetkeyForOutboxNumber());
        }
    }
}
