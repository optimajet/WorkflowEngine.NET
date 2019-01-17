using OptimaJet.Workflow.RavenDB;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.RavenDb.Helpers;

namespace WF.Sample.RavenDb.Implementation
{
    public class DocumentRepository : IDocumentRepository
    {
        private static DocumentStore Store => (WorkflowInit.Runtime.PersistenceProvider as RavenDBProvider).Store;

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            using (var session = Store.OpenSession())
            {
                var document = session.Load<Entities.Document>(id);
                if (document != null)
                {
                    document.State = nextState;
                    document.StateName = nextStateName;
                    session.SaveChanges();
                }
            }
        }

        public void Delete(Guid[] ids)
        {
            using (var session = Store.OpenSession())
            {
                var objs = session.Load<Entities.Document>(ids.Select(c => c as ValueType).ToList());
                foreach (var item in objs)
                {
                    session.Delete(item);
                }

                session.SaveChanges();
            }

            Entities.WorkflowInbox[] wis = null;
            do
            {
                foreach (var id in ids)
                {
                    using (var session = Store.OpenSession())
                    {
                        wis = session.Query<Entities.WorkflowInbox>().Where(c => c.ProcessId == id).ToArray();
                        foreach (var wi in wis)
                            session.Delete(wi);

                        session.SaveChanges();
                    }
                }
            } while (wis.Length > 0);
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<Entities.Document>(processId);
                if (doc != null)
                {
                    doc.TransitionHistories.RemoveAll(dth => !dth.TransitionTime.HasValue);
                    session.SaveChanges();
                }
            }
        }

        public List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            using (var session = Store.OpenSession())
            {
                var query = session.Advanced.LuceneQuery<Entities.Document>();
                int actual = page * pageSize;
                count = query.QueryResult.TotalResults;

                return session.Advanced
                    .LuceneQuery<Entities.Document>()
                    .Skip(actual)
                    .Take(pageSize).ToList().Select(x => Mappings.Mapper.Map<Document>(x)).ToList();
            }
        }

        public Document Get(Guid id, bool loadChildEntities = true)
        {
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<Entities.Document>(id);
                if (doc == null) return null;

                return Mappings.Mapper.Map<Document>(doc);
            }
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            var res = new List<string>();

            using (var session = Store.OpenSession())
            {
                var document = session.Load<Entities.Document>(documentId);
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
        }

        public List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            using (var session = Store.OpenSession())
            {
                var document = session.Load<Entities.Document>(id);
                if (document == null)
                    return null;

                return document.TransitionHistories.Select(h => Mappings.Mapper.Map<DocumentTransitionHistory>(h)).ToList();
            }
        }

        public List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
            string strGuid = identityId.ToString();

            using (var session = Store.OpenSession())
            {
                count = session.Query<Entities.WorkflowInbox>().Where(c => c.IdentityId == strGuid).Count();

                int actual = page * pageSize;

                var inbox = session
                    .Query<Entities.WorkflowInbox>()
                    .Where(c => c.IdentityId == strGuid)
                    .Skip(actual)
                    .Take(pageSize);

                var tmp = inbox.ToArray().Select(c => c.ProcessId as ValueType);
                var docs = session.Load<Entities.Document>(tmp).Select(x => Mappings.Mapper.Map<Document>(x));
                res.AddRange(docs);
            }
            return res;
        }

        public List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var res = new List<Document>();
            using (var session = Store.OpenSession())
            {

                if (Store.DatabaseCommands.GetIndex("DocumentByWFExecutorId") == null)
                {
                    Store.DatabaseCommands.PutIndex("DocumentByWFExecutorId", new IndexDefinition
                    {
                        Map = @"from doc in docs.Documents
                                    from transition in doc.TransitionHistories
                                    select new { WFExecutorId = transition.EmployeeId }"
                    });
                }

                count = session.Advanced.LuceneQuery<Entities.Document>("DocumentByWFExecutorId")
                               .Where("WFExecutorId:" + identityId.ToString()).QueryResult.TotalResults;

                int actual = page * pageSize;

                var docs = session.Advanced
                    .LuceneQuery<Entities.Document>("DocumentByWFExecutorId")
                    .Where("WFExecutorId:" + identityId.ToString())
                    .Skip(actual)
                    .Take(pageSize);

                actual = actual + pageSize;
                res.AddRange(docs.Select(x => Mappings.Mapper.Map<Document>(x)));
            }
            return res;
        }

        public Document InsertOrUpdate(Document doc)
        {
            Entities.Document target = null;

            using (var session = Store.OpenSession())
            {
                if (doc.Id != Guid.Empty)
                {
                    target = session.Load<Entities.Document>(doc.Id);
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
                        Number = GetNextNumber()
                    };
                    session.Store(target);

                    doc.Number = target.Number;
                    doc.Id = target.Id;
                }

                target.Name = doc.Name;
                target.ManagerId = doc.ManagerId;
                target.ManagerName = doc.Manager?.Name;
                target.Comment = doc.Comment;
                target.Sum = doc.Sum;

                session.SaveChanges();

                return doc;
            }
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            return GetAuthorsBoss(documentId).Contains(identityId.ToString());
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<Entities.Document>(id);

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

                if (employeeId.HasValue)
                {
                    var employee = session.Load<Entities.Employee>(employeeId.Value);
                    historyItem.EmployeeName = employee.Name;
                }
                else
                {
                    historyItem.EmployeeName = null;
                }

                session.SaveChanges();
            }
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            using (var session = Store.OpenSession())
            {
                var doc = session.Load<Entities.Document>(id);

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

                session.SaveChanges();
            }
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
            using (var session = Store.OpenSession())
            {
                var number = session.Load<Entities.SettingParam<int>>("settings/documentnumber");
                if (number == null)
                {
                    session.Store(new Entities.SettingParam<int> { Value = res + 1 }, "settings/documentnumber");
                }
                else
                {
                    res = number.Value;
                    number.Value += 1;
                }

                session.SaveChanges();
            }
            return res;
        }
    }
}
