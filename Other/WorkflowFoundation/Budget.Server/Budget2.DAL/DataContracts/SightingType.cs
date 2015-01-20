using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    public class SightingType
    {
        public byte Id { get; set; }

        public static readonly SightingType BillDemandLimitExecutorSighting = new SightingType {Id = 1};

        public static readonly SightingType BillDemandLimitManagerSighting = new SightingType { Id = 2 };
    }
}
