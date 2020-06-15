using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Sample.MongoDb.Entities
{
    public class WorkflowInbox
    {
        public Guid Id{ get; set; }
        public Guid ProcessId;
        public string IdentityId;
    }
}
