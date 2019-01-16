using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.MsSql
{
    public class DocumentRepository : IDocumentRepository
    {
   
        public Document InsertOrUpdate(Document doc)
        {
            using (var context = new Business.DataModelDataContext())
            {
                Business.Document target = null;
                if (doc.Id != Guid.Empty)
                {
                    target = context.Documents.SingleOrDefault(d => d.Id == doc.Id);
                    if (target == null)
                    {
                        return null;
                    }
                }
                else
                {
                    target = new Business.Document
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = doc.AuthorId,
                        StateName = doc.StateName,
                        State = "VacationRequestCreated"

                    };
                    context.Documents.InsertOnSubmit(target);
                }

                target.Name = doc.Name;
                target.ManagerId = doc.ManagerId;
                target.Comment = doc.Comment;
                target.Sum = doc.Sum;

                context.SubmitChanges();

                doc.Id = target.Id;
                doc.Number = target.Number;

                return doc;
            }
        }

        public void ChangeState(Guid id, string nextState, string nextStateName)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var document = Get(id, context);
                if (document == null)
                    return;

                document.State = nextState;
                document.StateName = nextStateName;
                context.SubmitChanges();
            }
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var existingNotUsedItems =
                    context.DocumentTransitionHistories.Where(
                        dth =>
                        dth.DocumentId == processId && !dth.TransitionTime.HasValue).ToList();

                context.DocumentTransitionHistories.DeleteAllOnSubmit(existingNotUsedItems);
                context.SubmitChanges();
            }
        }

        public List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var query = context.Documents.OrderByDescending(c => c.Number);
                count = query.Count();
                return query.Skip(actual).Take(pageSize).Select(d => Mappings.Mapper.Map<Document>(d)).ToList();
            }
        }

        public List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var subQuery = context.WorkflowInboxes.Where(c => c.IdentityId == identityId);
                var query = context.Documents.Where(c => subQuery.Any(i => i.ProcessId == c.Id));
                count = query.Count();
                return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                    .Select(d => Mappings.Mapper.Map<Document>(d)).ToList();
            }
        }

        public List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var subQuery = context.DocumentTransitionHistories.Where(c => c.EmployeeId == identityId);
                var query = context.Documents.Where(c => subQuery.Any(i => i.DocumentId == c.Id));
                count = query.Count();
                return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                    .Select(d => Mappings.Mapper.Map<Document>(d)).ToList();
            }
        }

        public List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.DocumentTransitionHistories.Where(h => h.DocumentId == id)
                    .OrderBy(h => h.TransitionTimeForSort).ThenBy(h => h.Order)
                    .Select(x => Mappings.Mapper.Map<DocumentTransitionHistory>(x)).ToList();
            }
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var historyItem = new Business.DocumentTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToEmployeeNames = GetEmployeesString(identities, context),
                    DestinationState = nextState,
                    DocumentId = id,
                    InitialState = currentState,
                    Command = command
                };

                context.DocumentTransitionHistories.InsertOnSubmit(historyItem);
                context.SubmitChanges();

            }
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var historyItem =
                    context.DocumentTransitionHistories.FirstOrDefault(
                        h => h.DocumentId == id && !h.TransitionTime.HasValue &&
                        h.InitialState == currentState && h.DestinationState == nextState);

                if (historyItem == null)
                {
                    historyItem = new Business.DocumentTransitionHistory
                    {
                        Id = Guid.NewGuid(),
                        AllowedToEmployeeNames = string.Empty,
                        DestinationState = nextState,
                        DocumentId = id,
                        InitialState = currentState
                    };

                    context.DocumentTransitionHistories.InsertOnSubmit(historyItem);

                }

                historyItem.Command = command;
                historyItem.TransitionTime = DateTime.Now;
                historyItem.EmployeeId = employeeId;

                context.SubmitChanges();
            }
        }

        private string GetEmployeesString(IEnumerable<string> identities, Business.DataModelDataContext context)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = context.Employees.Where(e => identitiesGuid.Contains(e.Id)).ToList();

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

        public Document Get(Guid id, bool loadChildEntities = true)
        {
            using (var context = new Business.DataModelDataContext())
            {
                return Mappings.Mapper.Map<Document>(Get(id, context, loadChildEntities));
            }
        }

        public IEnumerable<string> GetAuthorsBoss(Guid documentId)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == documentId);
                if (document == null)
                    return new List<string> { };

                return
                    context.vHeads.Where(h => h.Id == document.AuthorId)
                        .Select(h => h.HeadId)
                        .ToList()
                        .Select(c => c.ToString());
            }
        }

        public bool IsAuthorsBoss(Guid documentId, Guid identityId)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == documentId);
                if (document == null)
                    return false;
                return context.vHeads.Count(h => h.Id == document.AuthorId && h.HeadId == identityId) > 0;
            }
        }

        public void Delete(Guid[] ids)
        {
            using (var context = new Business.DataModelDataContext())
            {
                var objs =
                    (from item in context.Documents where ids.Contains(item.Id) select item).ToList();

                foreach (Business.Document item in objs)
                {
                    context.Documents.DeleteOnSubmit(item);
                }

                context.SubmitChanges();
            }
        }

        private Business.Document Get(Guid id, Business.DataModelDataContext context, bool loadChildEntities = true)
        {
            if(loadChildEntities) context.LoadOptions = GetDefaultDataLoadOptions();
            var doc = context.Documents.FirstOrDefault(c => c.Id == id);

            if (doc == null) return null;
            return doc;
        }

        private static DataLoadOptions GetDefaultDataLoadOptions()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Business.Document>(c => c.Employee);
            lo.LoadWith<Business.Document>(c => c.Employee1);
            lo.LoadWith<Business.DocumentTransitionHistory>(c => c.Employee);
            return lo;
        }
    }
}
