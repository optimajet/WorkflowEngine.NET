using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.API.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using Budget2.Server.Business.Interface.Services;
using System.ServiceModel;
using Common.WCF;

namespace Budget2.Server.API.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class ContractUploadApi : IContractUploadApi
    {
        public ContractUploadApi()
        {
            WebClientApplication.BuildItemWithCurrentContext(this);
        }

        [ServiceDependency]
        public IContractUploadBuinessService ContractUploadBuinessService { get; set; }

        public void UploadContract(Contract contract)
        {
            var fault = ContractUploadBuinessService.UploadContract(contract);
            if (fault != null)
                throw new FaultException<BaseFault>(fault);
        }

        public void UploadContracts(IEnumerable<Contract> contract)
        {
            var fault = ContractUploadBuinessService.UploadContract(contract);
            if (fault != null)
                throw new FaultException<BaseFault>(fault);
        }
    }
}
