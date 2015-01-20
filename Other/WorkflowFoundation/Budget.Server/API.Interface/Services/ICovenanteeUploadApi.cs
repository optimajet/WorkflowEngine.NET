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
    public interface ICovenanteeUploadApi
    {
        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void UploadCovenantee(Covenantee covenantee);
        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void UploadCovenantees(IEnumerable<Covenantee> covenantees);
    }
}
