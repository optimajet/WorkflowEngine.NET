using System.Collections.Generic;
using System.ServiceModel;
using Budget2.Server.API.Interface.DataContracts;
using Common.WCF;

namespace Budget2.Server.API.Interface.Services
{
    [ServiceContract]
    public interface IUpdateRatesApi
    {
        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void UpdateRates(IEnumerable<CurrencyRate> currencyRates);
    }
}
