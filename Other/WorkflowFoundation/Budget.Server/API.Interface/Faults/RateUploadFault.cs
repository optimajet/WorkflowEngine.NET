using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Common.WCF;

namespace Budget2.Server.API.Interface.Faults
{
    [DataContract]
    public class RateUploadFault : BaseFault
    {
        public RateUploadFault() : base((int)ErrorCodes.RateUploadError){}
    }
}
