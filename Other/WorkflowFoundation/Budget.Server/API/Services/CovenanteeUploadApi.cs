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
    public class CovenanteeUploadApi : ICovenanteeUploadApi
    {
        public CovenanteeUploadApi()
        {
            WebClientApplication.BuildItemWithCurrentContext(this);
        }

        [ServiceDependency]
        public ICovenanteeUploadBuinessService CovenanteeUploadBuinessService { get; set; }


        public void UploadCovenantee(Covenantee covenantee)
        {
            var fault = CovenanteeUploadBuinessService.UploadCovenantee(covenantee);
            if (fault != null)
                throw new FaultException<BaseFault>(fault);
        }

        public void UploadCovenantees(IEnumerable<Covenantee> covenantees)
        {
            var fault = CovenanteeUploadBuinessService.UploadCovenantee(covenantees);
            if (fault != null)
                throw new FaultException<BaseFault>(fault);
        }
        
    }
}
