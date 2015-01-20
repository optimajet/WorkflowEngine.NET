using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;
using Budget2.Server.API.Interface.DataContracts;
using Common.WCF;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IContractUploadBuinessService
    {
        BaseFault UploadContract(Contract contract);
        BaseFault UploadContract(IEnumerable<Contract> contract);
    }
}
