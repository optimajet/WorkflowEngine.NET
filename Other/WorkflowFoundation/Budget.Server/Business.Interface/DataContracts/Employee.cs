using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid IdentityId { get; set; }
    }
}
