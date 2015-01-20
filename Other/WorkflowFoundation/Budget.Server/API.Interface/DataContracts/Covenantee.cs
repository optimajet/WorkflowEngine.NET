using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public class Covenantee
    {
        [DataMember(IsRequired = true)]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string INN { get; set; }
        [DataMember]
        public string KPP { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string ContactInfo { get; set; }
        [DataMember]
        public string Director { get; set; }
        [DataMember]
        public string Account { get; set; }
        [DataMember]
        public string BankName { get; set; }
        [DataMember]
        public string BankBIC { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public long? IdCft { get; set; }
    }
}
