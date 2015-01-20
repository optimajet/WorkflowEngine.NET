using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
   public class PaymentPlanItem
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }
        [DataMember]
        public int DogId { get; set; }
        [DataMember]
        public string Prefix { get; set; }
        [DataMember]
        public int? Number { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public int? FromEmployee { get; set; }
        [DataMember]
        public int? ToEmployee { get; set; }
        [DataMember]
        public string PaymentTerms { get; set; }
        [DataMember]
        public DateTime? DateFrom { get; set; }
        [DataMember]
        public DateTime? DateTo { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public bool FactPay { get; set; }
    }
}
