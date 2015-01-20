using Common.WCF;
using System.Runtime.Serialization;

namespace Budget2.Server.API.Interface.Faults
{
    [DataContract]
    public class ContractUploadFault : BaseFault
    {
        public ContractUploadFault() : base((int)ErrorCodes.ContractUploadError) { }
    }
}
