using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WF.Sample.Business.Properties;

namespace WF.Sample.Business.Helpers
{

    public class DocumentHelper
    {
        public static List<Document> GetAll()
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.Documents.ToList();
            }
        }

        public static List<Document> Get(out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var query = context.Documents.OrderByDescending(c => c.Number);
                count = query.Count();
                return query.Skip(actual).Take(pageSize).ToList();
            }
        }

        public static List<Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var subQuery = context.WorkflowInboxes.Where(c => c.IdentityId == identityId);
                var query = context.Documents.Where(c => subQuery.Any(i => i.ProcessId == c.Id));
                count = query.Count();
                return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize).ToList();
            }
        }

        public static List<Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                int actual = page * pageSize;
                var subQuery = context.DocumentTransitionHistories.Where(c => c.EmployeeId == identityId);
                var query = context.Documents.Where(c => subQuery.Any(i => i.DocumentId == c.Id));
                count = query.Count();
                return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize).ToList();
            }
        }

        public static List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.DocumentTransitionHistories.Where(h=>h.DocumentId == id).OrderBy(h=>h.TransitionTimeForSort).ThenBy(h=>h.Order).ToList();
            }
        }

        public static Document Get(Guid id)
        {
            using (var context = new DataModelDataContext())
            {
                return Get(id, context);
            }
        }

        public static Document Get(Guid id, DataModelDataContext context)
        {
            context.LoadOptions = GetDefaultDataLoadOptions();
            return context.Documents.OrderBy(c => c.Number).FirstOrDefault(c => c.Id == id);
        }

       

        private static DataLoadOptions GetDefaultDataLoadOptions()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Document>(c => c.Employee);
            lo.LoadWith<Document>(c => c.Employee1);
            lo.LoadWith<DocumentTransitionHistory>(c=>c.Employee);
            return lo;
        }

        public static void Delete(Guid[] ids)
        {
            using (var context = new DataModelDataContext())
            {
                var objs =
                    (from item in context.Documents where ids.Contains(item.Id) select item).ToList();

                foreach (Document item in objs)
                {
                    context.Documents.DeleteOnSubmit(item);
                }

                context.SubmitChanges();
            }
        }       
    }
}
