using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web;
using WF.Sample.Business.Models;
using WF.Sample.Business.Workflow;

namespace WF.Sample.Business.Helpers
{
    public class EmployeeHelper
    {
        public static object _lock = new object();

        private static List<Employee> _employeeAll = null;
        public static List<Employee> EmployeeCache
        {
            get
            {
                if (_employeeAll == null)
                {
                    lock (_lock)
                    {
                        if (_employeeAll == null)
                        {
                            _employeeAll = GetAll();
                        }
                    }
                }

                return _employeeAll;
            }
        }

        public static List<Employee> GetAll()
        {
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Employee>("Employee");
            return dbcoll.FindAll().ToList();
        }

        public static string GetNameById(Guid id)
        {
            string res = "Unknown";
            var dbcoll = Workflow.WorkflowInit.Provider.Store.GetCollection<Employee>("Employee");
            var item = dbcoll.FindOneById(id);
            if (item != null)
            {
                res = item.Name;
            }
            return res;
        }

        public static string GetListRoles(Employee item)
        {
            return string.Join(",", item.Roles.Select(c => c.Value).ToArray());
        }
    }
}
