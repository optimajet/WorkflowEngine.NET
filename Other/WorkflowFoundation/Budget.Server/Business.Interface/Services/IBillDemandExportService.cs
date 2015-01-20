using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IBillDemandExportService
    {
        void ExportBillDemand(BillDemandForExport billDemandForExport);
        BillDemandExternalState GetBillDemandExternalStaus(Guid billDemandUid);
    }
}
