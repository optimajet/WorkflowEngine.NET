using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Budget2.Server.API.Interface.DataContracts
{
     [DataContract]
    public abstract class BaseApiCommandArgument
    {
         [DataMember]
        public Guid SecurityToken { get; set; }
         [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Sign { get; set; }
    }

    [DataContract]
    public class ApiCommandArgument : BaseApiCommandArgument
    {
        [DataMember]
        public Guid InstanceId { get; set; }
    }

    [DataContract]
    public class ApiMassCommandEventArg : BaseApiCommandArgument
    {
        [DataMember]
        public IEnumerable<Guid> InstanceIds { get; set; }
    }

    [DataContract]
    public class SetStateApiCommandArgument : ApiCommandArgument
    {
        [DataMember]
        public string StateNameToSet { get; set; }
    }

    [DataContract]
    public class  BillDemandPaidApiCommandArgument : ApiCommandArgument
    {
        [DataMember]
        public DateTime? PaymentDate { get; set; }

        [DataMember]
        public string DocumentNumber { get; set; }
    }
}
