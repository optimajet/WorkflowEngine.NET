using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Budget2.Server.API.Interface.DataContracts
{
    [DataContract]
    public  class WorkflowStateInfo
    {
        [DataMember]
        public string StateSystemName { get; set; }

        [DataMember]
        public string StateVisibleName { get; set; }

        public Guid WorkflowTypeId { get; set; } 
    }
}
