using System.Collections.Generic;
using Budget2.Server.API.Interface.DataContracts;
using Common.WCF;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IUpdateRatesService
    {
        BaseFault UpdateRates(IEnumerable<CurrencyRate> currencyRates);
    }
}
