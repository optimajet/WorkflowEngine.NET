using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.API.Interface.Services;
using Common.WCF;
using Microsoft.Practices.CompositeWeb;
using Budget2.Server.Business.Interface.Services;

namespace Budget2.Server.API.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UpdateRatesApi : IUpdateRatesApi
    {
        public UpdateRatesApi()
        {
            WebClientApplication.BuildItemWithCurrentContext(this);
        }

        [ServiceDependency]
        public IUpdateRatesService UpdateRatesBuinessService { get; set; }

        public void UpdateRates(IEnumerable<CurrencyRate> currencyRates)
        {
            var fault = UpdateRatesBuinessService.UpdateRates(currencyRates);
            if (fault != null)
                throw new FaultException<BaseFault>(fault);
        }
    }
}