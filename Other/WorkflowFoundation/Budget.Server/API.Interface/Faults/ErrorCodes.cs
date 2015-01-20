using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.API.Interface.Faults
{
    /// <summary>
    /// Нумерация ошибок - snn 
    /// s - Номер сервиса
    /// nn - Номер ошибки
    /// </summary>
    public enum ErrorCodes : int
    {      
        Empty = -1,
        BillDemandUploadError = 101,
        CurrencyUploadError = 201,
        RateUploadError = 202,
        CovenanteeUploadError = 302,
        ContractUploadError = 402,
        CommandProccessingError = 001

    }
}
