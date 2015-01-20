using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public class CurrencyRate
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember(IsRequired = true)]
        public string ISOCode { get; set; }
        [DataMember(IsRequired = true)]
        public int Measure { get; set; }
        [DataMember(IsRequired = true)]
        public decimal Rate { get; set; }
        [DataMember(IsRequired = true)]
        public string BaseCurrencyISOCode { get; set; }
        [DataMember]
        public string BaseCurrencyName { get; set; }
        [DataMember(IsRequired = true)]
        public DateTime RevaluationDate { get; set; }
    }
}
