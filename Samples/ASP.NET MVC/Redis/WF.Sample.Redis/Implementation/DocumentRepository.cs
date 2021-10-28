﻿using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using Employee = WF.Sample.Redis.Entities.Employee;

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

        public List<Document> GetByIds(List<Guid> ids)
        {
            var db = _connector.GetDatabase();
            var docs = db.StringGet(ids.Select(k => (RedisKey)GetKeyForDocument(k)).ToArray());
            return docs.Where(x=>x.HasValue).Select(d => Mappings.Mapper.Map<Document>(JsonConvert.DeserializeObject<Entities.Document>(d))).ToList();
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

                batch.HashDeleteAsync(GetKeyForDocumentIdsNumber(), doc.Number.ToString());
                batch.KeyDeleteAsync(employeesKey);
            }

            batch.KeyDeleteAsync(ids.Select(k => (RedisKey)GetKeyForDocument(k)).ToArray());

            batch.KeyDeleteAsync(ids.Select(k => (RedisKey)GetKeyForInboxEmployees(k)).ToArray());

            batch.SortedSetRemoveAsync(GetKeyForSortedDocuments(), ids.Select(i => (RedisValue)i.ToString()).ToArray());

            batch.Execute();
        }

        public List<Document> Get(out int count, int page = 1, int pageSize = 128)
        {
            page -= 1;
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
        
        public Business.Model.Document GetByNumber(int number)
        {
            var db = _connector.GetDatabase();

            var documentId = db.HashGet(GetKeyForDocumentIdsNumber(), number.ToString());
            var documentJson = db.StringGet(GetKeyForDocument(Guid.Parse(documentId.ToString())));
            var document = JsonConvert.DeserializeObject<Entities.Document>(documentJson.ToString());
            
            return Mappings.Mapper.Map<Document>(document);
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
        
        public Document InsertOrUpdate(Document doc)
        {
            Entities.Document target = null;
            
            var db = _connector.GetDatabase();
            var batch = db.CreateBatch();

            if (String.IsNullOrEmpty(doc.Manager?.Name) && doc.Manager!=null)
            {
                Employee manager = GetEmployee(db, doc.Manager.Id);
                doc.Manager.Name = manager?.Name;
            }
            
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
                batch.HashSetAsync(GetKeyForDocumentIdsNumber(), target.Number.ToString(), target.Id.ToString("N") );
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
