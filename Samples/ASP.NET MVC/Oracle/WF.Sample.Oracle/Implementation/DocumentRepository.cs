using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using System.Data.Entity;

namespace WF.Sample.Oracle.Implementation
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly SampleContext _sampleContext;

        public DocumentRepository(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            var document = GetDocument(id);
            if (document == null)
                return;

            document.State = nextState;
            document.StateName = nextStateName;
            
            _sampleContext.SaveChanges();
        }

        public void Delete(Guid[] ids)
        {
            var objs = _sampleContext.Documents.Where(x => ids.Contains(x.Id));
             
            _sampleContext.Documents.RemoveRange(objs);

            _sampleContext.SaveChanges();
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            var existingNotUsedItems =
                   _sampleContext.DocumentTransitionHistories.Where(
                       dth =>
                       dth.DocumentId == processId && !dth.TransitionTime.HasValue);

            _sampleContext.DocumentTransitionHistories.RemoveRange(existingNotUsedItems);

            _sampleContext.SaveChanges();
        }

        public List<Business.Model.Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var query = _sampleContext.Documents.OrderByDescending(c => c.Number);
            count = query.Count();
            return query.Include(x => x.Author)
                        .Include(x => x.Manager)
                        .Skip(actual)
                        .Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            var document = _sampleContext.Documents.Find(documentId);
            if (document == null)
                return new List<string> { };

            return
                _sampleContext.VHeads.Where(h => h.Id == document.AuthorId)
                    .Select(h => h.HeadId)
                    .ToList()
                    .Select(c => c.ToString());
        }

        public List<Business.Model.DocumentTransitionHistory> GetHistory(Guid id)
        {
            DateTime orderTime = new DateTime(9999, 12, 31);

            return _sampleContext.DocumentTransitionHistories
                 .Include(x => x.Employee)
                 .Where(h => h.DocumentId == id)
                 .OrderBy(h => h.TransitionTime == null ? orderTime : h.TransitionTime.Value)
                 .ThenBy(h => h.Order)
                 .ToList()
                 .Select(x => Mappings.Mapper.Map<Business.Model.DocumentTransitionHistory>(x)).ToList();
        }

        public List<Business.Model.Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var strGuid = identityId.ToString();
            int actual = page * pageSize;
            var subQuery = _sampleContext.WorkflowInboxes.Where(c => c.IdentityId == strGuid);

            var query = _sampleContext.Documents.Include(x => x.Author)
                                                .Include(x => x.Manager)
                                                .Where(c => subQuery.Any(i => i.ProcessId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public List<Business.Model.Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var subQuery = _sampleContext.DocumentTransitionHistories.Where(c => c.EmployeeId == identityId);
            var query = _sampleContext.Documents.Include(x => x.Author)
                                                .Include(x => x.Manager)
                                                .Where(c => subQuery.Any(i => i.DocumentId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                .ToList()
                .Select(d => Mappings.Mapper.Map<Business.Model.Document>(d)).ToList();
        }

        public Business.Model.Document InsertOrUpdate(Business.Model.Document doc)
        {
            Document target = null;
            if (doc.Id != Guid.Empty)
            {
                target = _sampleContext.Documents.Find(doc.Id);
                if (target == null)
                {
                    return null;
                }
            }
            else
            {
                target = new Document
                {
                    Id = Guid.NewGuid(),
                    AuthorId = doc.AuthorId,
                    StateName = doc.StateName
                };
                _sampleContext.Documents.Add(target);
            }

            target.Name = doc.Name;
            target.ManagerId = doc.ManagerId;
            target.Comment = doc.Comment;
            target.Sum = doc.Sum;

            _sampleContext.SaveChanges();

            doc.Id = target.Id;
            doc.Number = target.Number;

            return doc;
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            var document = _sampleContext.Documents.Find(documentId);
            if (document == null)
                return false;
            return _sampleContext.VHeads.Count(h => h.Id == document.AuthorId && h.HeadId == identityId) > 0;
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            var historyItem =
              _sampleContext.DocumentTransitionHistories.FirstOrDefault(
                  h => h.DocumentId == id && !h.TransitionTime.HasValue &&
                  h.InitialState == currentState && h.DestinationState == nextState);

            if (historyItem == null)
            {
                historyItem = new DocumentTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = string.Empty,
                    DestinationState = nextState,
                    DocumentId = id,
                    InitialState = currentState
                };

                _sampleContext.DocumentTransitionHistories.Add(historyItem);

            }

            historyItem.Command = command;
            historyItem.TransitionTime = DateTime.Now;
            historyItem.EmployeeId = employeeId;

            _sampleContext.SaveChanges();
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            var historyItem = new DocumentTransitionHistory
            {
                Id = Guid.NewGuid(),
                AllowedToEmployeeNames = GetEmployeesString(identities),
                DestinationState = nextState,
                DocumentId = id,
                InitialState = currentState,
                Command = command
            };

            _sampleContext.DocumentTransitionHistories.Add(historyItem);
            _sampleContext.SaveChanges();
        }

        public Business.Model.Document Get(Guid id, bool loadChildEntities = true)
        {
            Document document = GetDocument(id, loadChildEntities);
            if (document == null) return null;
            return Mappings.Mapper.Map<Business.Model.Document>(document);
        }

        private Document GetDocument(Guid id, bool loadChildEntities = true)
        {
            Document document = null;

            if (!loadChildEntities)
            {
                document = _sampleContext.Documents.Find(id);
            }
            else
            {
                document = _sampleContext.Documents
                                         .Include(x => x.Author)
                                         .Include(x => x.Manager).FirstOrDefault(x => x.Id == id);
            }

            return document;

        }

        private string GetEmployeesString(IEnumerable<string> identities)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = _sampleContext.Employees.Where(e => identitiesGuid.Contains(e.Id)).ToList();

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
    }
}
