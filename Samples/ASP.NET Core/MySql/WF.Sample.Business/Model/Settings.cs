using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Business.Model
{
    public class Settings
    {
        public string WFSchema { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Role> Roles { get; set; }
        public List<StructDivision> StructDivision { get; set; }

        public string SchemeName
        {
            get { return "SimpleWF"; }
        }
    }
}
