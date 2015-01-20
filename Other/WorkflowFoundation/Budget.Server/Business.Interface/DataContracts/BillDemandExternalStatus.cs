using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    [Serializable]
    public class BillDemandExternalStatus
    {
        public BillDemandExternalStatus(int code)
        {
            Code = code;
        }


        public int Code { get; set; }
        public string Name { get; private set; }
        public bool RequiresDescription { get; private set; }
        public bool IsIgnored { get; private set; }

        public static readonly BillDemandExternalStatus Rejected = new BillDemandExternalStatus(0) { Name = "Отбракован", RequiresDescription = true, IsIgnored = false};
        public static readonly BillDemandExternalStatus Accepted = new BillDemandExternalStatus(1) { Name = "Принят в АБС", RequiresDescription = false, IsIgnored = false };
        public static readonly BillDemandExternalStatus Destroyed = new BillDemandExternalStatus(2) { Name = "Ликвидирован", RequiresDescription = true, IsIgnored = false };
        public static readonly BillDemandExternalStatus WaitSending = new BillDemandExternalStatus(3) { Name = "Ожидает отправки", RequiresDescription = false, IsIgnored = false };
        public static readonly BillDemandExternalStatus Paid = new BillDemandExternalStatus(4) { Name = "Проведен/Отправлен", RequiresDescription = false, IsIgnored = false };
        public static readonly BillDemandExternalStatus Loaded = new BillDemandExternalStatus(5) { Name = "Загружен в БОСС", RequiresDescription = false, IsIgnored = false };
        public static readonly BillDemandExternalStatus RejectedInBOSS = new BillDemandExternalStatus(7) { Name = "Отбракован в БОСС", RequiresDescription = true, IsIgnored = false };
        public static readonly BillDemandExternalStatus Processing = new BillDemandExternalStatus(10){IsIgnored = true};
        public static readonly BillDemandExternalStatus Unknown = new BillDemandExternalStatus(-1) { IsIgnored = true };

        public static BillDemandExternalStatus[] All = new BillDemandExternalStatus[]{Rejected,Accepted,Destroyed,WaitSending,Paid,Loaded,RejectedInBOSS,Processing,Unknown};
    }
}
