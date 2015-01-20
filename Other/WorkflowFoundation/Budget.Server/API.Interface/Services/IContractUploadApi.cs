using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Budget2.Server.API.Interface.DataContracts;
using Common.WCF;

namespace Budget2.Server.API.Interface.Services
{
    [ServiceContract]
    public interface IContractUploadApi
    {
        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void UploadContract(Contract contract);

        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void UploadContracts(IEnumerable<Contract> contract);
    }
}
