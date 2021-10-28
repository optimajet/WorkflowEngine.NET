using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.Redis.Implementation
{
    public class EmployeeRepository : RepositoryBase, IEmployeeRepository
    {
  
        public EmployeeRepository(ConnectionSettingsProvider settings) : base(settings)
        {
        }

        public bool CheckRole(Guid employeeId, string roleName)
        {
            var db = _connector.GetDatabase();
            var emp = GetEmployee(db, employeeId);
            return emp.Roles.Any(c => c.Value == roleName);
        }

        public List<Employee> GetAll()
        {
            return GetAll(_connector.GetDatabase());
        }
        
        public List<Employee> GetWithPaging(string userName = null, SortDirection sortDirection = SortDirection.Asc, Paging paging = null)
        {
            var db = _connector.GetDatabase();

            var names = db.SetMembers(GetKeyForEmployeeNamesSet()).Select(n => n.ToString()).ToList();
            var resultNames = new List<string>();
            
            if (!string.IsNullOrEmpty(userName))
            {
                resultNames.AddRange(names.Where(name => name.Contains(userName)));
            }
            else
            {
                resultNames = names;
            }

            resultNames = sortDirection == SortDirection.Desc
                ? resultNames.OrderByDescending(x => x).ToList()
                : resultNames.OrderBy(x => x).ToList();

            if (paging != null)
            {
                resultNames = resultNames.Skip(paging.PageSize * (paging.PageIndex - 1))
                    .Take(paging.PageSize).ToList();
            }

            var ids = new List<Guid>();
            foreach (var nameKey in resultNames)
            {
                ids.AddRange(db.SetMembers(GetKeyForEmployeeIdsNameSet(nameKey)).Select(id => Guid.Parse(id)));
            }
            var employees = db.StringGet(ids.Select(id => (RedisKey)GetKeyForEmployee(id)).ToArray());
            
            return employees.Where(x=>x.HasValue).Select(e => Mappings.Mapper.Map<Employee>(JsonConvert.DeserializeObject<Entities.Employee>(e))).ToList();
        }
        
        internal List<Employee> GetAll(IDatabase db)
        {
            var keys = db.SetMembers(GetKeyForEmployeesSet());

            var employees = db.StringGet(keys.Select(k => (RedisKey)GetKeyForEmployee(new Guid((string)k))).ToArray());

            return employees.Select(d => Mappings.Mapper.Map<Employee>(JsonConvert.DeserializeObject<Entities.Employee>(d))).ToList();
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            var db = _connector.GetDatabase();

            var keys = db.SetMembers(GetKeyForEmployeesInRole(roleName));

            var employees = db.StringGet(keys.Select(k => (RedisKey)GetKeyForEmployee(new Guid((string)k))).ToArray());

            return employees.Select(d => JsonConvert.DeserializeObject<Entities.Employee>(d)).Select(c => c.Id.ToString()).ToList();
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";

            var db = _connector.GetDatabase();

            var item = GetEmployee(db, id);

            if (item != null)
                res = item.Name;

            return res;
        }
    }
}
