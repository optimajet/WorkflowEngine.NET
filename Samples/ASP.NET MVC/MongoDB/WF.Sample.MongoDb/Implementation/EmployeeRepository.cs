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
    public class EmployeeRepository : IEmployeeRepository
    {
        private static IMongoDatabase Store => (WorkflowInit.Runtime.PersistenceProvider as MongoDBProvider).Store;

        public bool CheckRole(Guid employeeId, string roleName)
        {
            var emp = CacheHelper<Entities.Employee>.Cache.FirstOrDefault(c => c.Id == employeeId);
            return emp.Roles.Any(c => c.Value == roleName);
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
