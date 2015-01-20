using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Budget2.Server.Security.Interface.DataContracts;

namespace Budget2.Server.API.Interface.DataContracts
{
    public interface IMassCommand
    {
        IEnumerable<Guid> InstanceIds { get; set; }
        string Comment { get; set; }
        WorkflowCommandType Command { get; }
    }

    public interface IContainsStateName
    {
        string StateNameToSet { get; set; }
    }

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
    public class ApiMassCommandEventArg : BaseApiCommandArgument, IMassCommand
    {
        [DataMember]
        public IEnumerable<Guid> InstanceIds { get; set; }

        [DataMember]
        public WorkflowCommandType Command { get; set; }
    }

    [DataContract]
    public class SetStateApiCommandArgument : ApiCommandArgument, IContainsStateName
    {
        [DataMember]
        public string StateNameToSet { get; set; }

    }

    [DataContract]
    public class ApiMassSetStateCommandEventArg : SetStateApiCommandArgument, IMassCommand
    {
        [DataMember]
        public IEnumerable<Guid> InstanceIds { get; set; }

        public WorkflowCommandType Command
        {
            get { return WorkflowCommandType.SetWorkflowState; }
        }
    }

    [DataContract]
    public class BillDemandPaidApiCommandArgument : ApiCommandArgument
    {
        [DataMember]
        public DateTime? PaymentDate { get; set; }

        [DataMember]
        public string DocumentNumber { get; set; }
    }
}
