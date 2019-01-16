using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.MsSql
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public List<Employee> GetAll()
        {
            using (var context = new Business.DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.Employees.ToList().Select(e => Mappings.Mapper.Map<Employee>(e)).OrderBy(c => c.Name).ToList();
            }
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";
            using (var context = new Business.DataModelDataContext())
            {
                var item = context.Employees.FirstOrDefault(c => c.Id == id);
                if (item != null)
                    res = item.Name;
            }
            return res;
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            using (var context = new Business.DataModelDataContext())
            {
                return
                    context.EmployeeRoles.Where(r => r.Role.Name == roleName).ToList()
                        .Select(r => r.EmployeeId.ToString()).ToList();
            }
        }

        public bool CheckRole(Guid employeeId, string roleName)
        {
            using (var context = new Business.DataModelDataContext())
            {
                return context.EmployeeRoles.Any(r => r.EmployeeId == employeeId && r.Role.Name == roleName);
            }
        }

        private DataLoadOptions GetDefaultDataLoadOptions()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Business.Employee>(c => c.StructDivision);
            lo.LoadWith<Business.EmployeeRole>(c => c.Role);
            lo.LoadWith<Business.Employee>(c => c.EmployeeRoles);
            return lo;
        }
    }
}
