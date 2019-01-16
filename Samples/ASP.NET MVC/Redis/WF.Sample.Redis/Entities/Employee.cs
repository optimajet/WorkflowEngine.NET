using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Sample.Redis.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Name;
        public Dictionary<Guid, string> Roles = new Dictionary<Guid, string>();

        public bool IsHead { get; set; }

        public Guid StructDivisionId;
        public string StructDivisionName;
    }
}
