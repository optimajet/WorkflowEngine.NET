using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class BillDemandExternalState 
    {
        public BillDemandExternalStatus Status { get; set; }
        public DateTime PaymentDate {get;set;}
        public string DocumentNumber { get; set; }
        public string Description { get; set; }
    }
}
