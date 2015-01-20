using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    public class DemandAdjustmentType
    {
        public byte Id { get; private set; }

        public static readonly DemandAdjustmentType Sequestering = new DemandAdjustmentType() {Id = 2};
        public static readonly DemandAdjustmentType Redistribution = new DemandAdjustmentType() {Id = 1};
        public static readonly DemandAdjustmentType Replanning = new DemandAdjustmentType() {Id = 3};

        public static readonly DemandAdjustmentType[] All = new[]
                                                                {
                                                                    Sequestering,
                                                                    Redistribution,
                                                                    Replanning
                                                                };
    }
}