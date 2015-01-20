using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public class CommandExecutionStatus
    {
        [DataMember]
        public Guid InstanceId { get; set; }
        [DataMember]
        public CommandExecutionResult Result { get; set; }


    }
}
