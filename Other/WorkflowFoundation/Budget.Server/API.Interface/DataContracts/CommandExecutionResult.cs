using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public enum CommandExecutionResult
    {
        [EnumMember]
        OK,
        [EnumMember]
        BillDemandExportPaymentPlanNotSelected,
        [EnumMember]
        Error

    }
}
