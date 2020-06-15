using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using System.Data.Entity;

namespace WF.Sample.Oracle.Implementation
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SampleContext _sampleContext;

        public EmployeeRepository(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        public bool CheckRole(Guid employeeId, string roleName)
        {
            return _sampleContext.EmployeeRoles.Any(r => r.EmployeeId == employeeId && r.Role.Name == roleName);
        }

        public List<Business.Model.Employee> GetAll()
        {
            return _sampleContext.Employees
                                    .Include(x => x.StructDivision)
                                    .Include(x => x.EmployeeRoles.Select(er => er.Role))
                                    .ToList().Select(e => Mappings.Mapper.Map<Business.Model.Employee>(e))
                                    .OrderBy(c => c.Name).ToList();
          
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            return
                  _sampleContext.EmployeeRoles.Where(r => r.Role.Name == roleName).ToList()
                      .Select(r => r.EmployeeId.ToString()).ToList();
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";
            
            var item = _sampleContext.Employees.Find(id);
            if (item != null)
                res = item.Name;
            
            return res;
        }
    }
}
