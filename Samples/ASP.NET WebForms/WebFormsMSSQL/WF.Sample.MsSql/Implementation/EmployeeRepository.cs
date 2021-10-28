using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;


namespace WF.Sample.MsSql.Implementation
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
        
        public List<Business.Model.Employee> GetWithPaging(string userName = null, SortDirection sortDirection = SortDirection.Asc, Paging paging = null)
        {
            List<Business.Model.Employee> result;
            IQueryable<Employee> data;
            if (userName == null)
            {
                data = (from emp in _sampleContext.Employees select emp);
            }
            else
            {
                var userNameUpper = userName.ToUpper();
                data = (from emp in _sampleContext.Employees
                    where emp.Name.ToUpper().Contains(userNameUpper)
                    select emp);
            }

            var sortData = sortDirection == SortDirection.Asc ?  data.OrderBy(e => e.Name) :  data.OrderByDescending(e => e.Name);

            if (paging == null)
            {
                return sortData.ToList().Select(e => Mappings.Mapper.Map<Business.Model.Employee>(e)).ToList();
            }
           
            return sortData.Skip(paging.PageSize * (paging.PageIndex - 1))
                .Take(paging.PageSize).ToList().Select(e => Mappings.Mapper.Map<Business.Model.Employee>(e)).ToList();
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
