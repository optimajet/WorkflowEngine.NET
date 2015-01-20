using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Common.WCF;

namespace Budget2.Server.API.Interface.Faults
{
    [DataContract]
    public class CurrencyUploadFault : BaseFault
    {
        public CurrencyUploadFault() : base((int)ErrorCodes.CurrencyUploadError) { }
    }
}
