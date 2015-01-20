using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class LimitSighter
    {
        public Guid LimitId { get; set; }
        public Guid SighterId { get; set; }
        public Guid InitiatorId { get; set; }
    }
}
