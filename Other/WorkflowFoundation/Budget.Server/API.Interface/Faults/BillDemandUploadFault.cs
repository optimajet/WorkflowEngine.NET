using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.WCF;

namespace Budget2.Server.API.Interface.Faults
{
    public class BillDemandUploadFault : BaseFault
    {
        public BillDemandUploadFault() : base((int)ErrorCodes.BillDemandUploadError) { }
    }
}
