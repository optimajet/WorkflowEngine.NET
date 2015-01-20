using System;

namespace Budget2.DAL.DataContracts
{
    [Serializable]
    public class PaymentKind
    {
        public byte Id { get; set; }

        public static readonly PaymentKind Cashless = new PaymentKind { Id = 0 };

        public static readonly PaymentKind Cash = new PaymentKind { Id = 1 };

        public static PaymentKind[] All = new PaymentKind[] {Cashless, Cash};

    }
}
