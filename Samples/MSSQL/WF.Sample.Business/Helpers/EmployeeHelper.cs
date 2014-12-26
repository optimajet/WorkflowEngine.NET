using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace WF.Sample.Business.Helpers
{
    public class EmployeeHelper
    {
        public static List<Employee> GetAll()
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.Employees.OrderBy(c=>c.Name).ToList();
            }
        }

        public static string GetNameById(Guid id)
        {
            string res = "Unknown";
            using (var context = new DataModelDataContext())
            {
                var item = context.Employees.FirstOrDefault(c => c.Id == id);
                if (item != null)
                    res = item.Name; 
            }
            return res;
        }

        public static string GetListRoles(Employee item)
        {
            return string.Join(",", item.EmployeeRoles.Select(c => c.Role.Name).ToArray());
        }

        private static DataLoadOptions GetDefaultDataLoadOptions()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Employee>(c => c.StructDivision);
            lo.LoadWith<EmployeeRole>(c => c.Role);
            lo.LoadWith<Employee>(c => c.EmployeeRoles);
            return lo;
        }
    }
}
