using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web;
using WF.Sample.Business.Models;
using WF.Sample.Business.Workflow;
using MongoDB.Driver;
using MongoDB.Bson;

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
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Employee>("Employee");
            return dbcoll.Find(new BsonDocument()).ToList();
        }

        public static string GetNameById(Guid id)
        {
            string res = "Unknown";
            var dbcoll = WorkflowInit.Provider.Store.GetCollection<Employee>("Employee");
            var item = dbcoll.Find(x => x.Id == id).FirstOrDefault();
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
