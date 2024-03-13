using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;

namespace WF.Sample.Business.DataAccess
{
    public interface IEmployeeRepository
    {
        List<Model.Employee> GetAll();
        string GetNameById(Guid id);
        IEnumerable<string> GetInRole(string roleName);
        bool CheckRole(Guid employeeId, string roleName);
        List<Model.Employee> GetWithPaging(string userName = null, SortDirection sortDirection = SortDirection.Asc, Paging paging = null);
    }
}
