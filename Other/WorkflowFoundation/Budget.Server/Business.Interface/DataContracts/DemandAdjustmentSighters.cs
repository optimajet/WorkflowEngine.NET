using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class DemandAdjustmentSighters
    {
        public Guid? SourceDemandLimitExecutor { get; set; }
        public Guid? SourceDemandLimitManager { get; set; }
        //public Guid? TargetDemandLimitExecutor { get; set; }
        //public Guid? TargetDemandLimitManager { get; set; }
    }
}
