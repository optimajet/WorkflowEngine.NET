using MongoDB.Driver;
using OptimaJet.Workflow.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.MongoDb.Helpers;
using SortDirection = OptimaJet.Workflow.Core.Persistence.SortDirection;

namespace WF.Sample.MongoDb.Implementation
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private static IMongoDatabase Store => (WorkflowInit.Runtime.PersistenceProvider as MongoDBProvider).Store;

        public bool CheckRole(Guid employeeId, string roleName)
        {
            var emp = CacheHelper<Entities.Employee>.Cache.Where(c => c.Id == employeeId).FirstOrDefault();
            return emp.Roles.Any(c => c.Value == roleName);
        }

        public List<Employee> GetWithPaging(string userName = null, SortDirection sortDirection = SortDirection.Asc,
            Paging paging = null)
        {
            var dbcoll = Store.GetCollection<Entities.Employee>("Employee");
            var employeesQuery = dbcoll.AsQueryable();
            List<Entities.Employee> employees;
            
            if (paging is null && userName is null)
            {
                employees = employeesQuery.ToList();
            }
            else
            {
                IOrderedQueryable<Entities.Employee> query = employeesQuery.OrderBy(e => e.Name);

                if (paging == null)
                {
                    employees = query.ToList();
                }
                else
                {
                    employees = query.Skip(paging.SkipCount())
                        .Take(paging.PageSize).ToList();
                }

            }
            return employees.Select(e => Mappings.Mapper.Map<Employee>(e)).ToList();
        }
        
        public List<Employee> GetAll()
        {
            return CacheHelper<Entities.Employee>.Cache.Select(e => Mappings.Mapper.Map<Employee>(e)).ToList();
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            return CacheHelper<Entities.Employee>.Cache.Where(c => c.Roles.Any(r => r.Value == roleName))
                                                       .Select(c => c.Id.ToString()).ToList();
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";

            var item = CacheHelper<Entities.Employee>.Cache.FirstOrDefault(e => e.Id == id);
            if (item != null)
                res = item.Name;

            return res;
        }
    }
}
