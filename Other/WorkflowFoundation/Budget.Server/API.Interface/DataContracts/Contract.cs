using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public class Contract
    {
        [DataMember(IsRequired = true)]
        public string Id { get; set; }
        [DataMember]
        public string Prefix { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public DateTime? Date { get; set; }
        [DataMember]
        public int? CustomerId { get; set; }
        [DataMember]
        public int? ExecutorId { get; set; }
        [DataMember]
        public string CustomerDetails { get; set; }
        [DataMember]
        public string ExecutorDetails { get; set; }
        [DataMember]
        public string Manager { get; set; }
        [DataMember]
        public string PaymentTerms { get; set; }
        [DataMember]
        public DateTime? DateFrom { get; set; }
        [DataMember]
        public DateTime? DateTo { get; set; }
        [DataMember]
        public DateTime? CompletionDate { get; set; }
        [DataMember]
        public decimal? Amount { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public IEnumerable<Contract> Subcontracts { get; set; }
        [DataMember]
        public IEnumerable<PaymentPlanItem> PaymentPlan { get; set; }
        [DataMember]
        public decimal? Rate { get; set; }
        [DataMember]
        public decimal? RateCBRPercents { get; set; }

    }
}
