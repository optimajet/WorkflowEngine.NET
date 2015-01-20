using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class TicketInfo
    {
        public Guid EntityId { get; set;}
        public Guid IdentityId { get; set; }
        public bool IsUsed { get; set; }
        public string ValidStateName { get; set;}
    }
}
