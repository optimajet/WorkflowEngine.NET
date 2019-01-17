#if !NETCOREAPP
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Data.Linq;
using System.Linq;
using OptimaJet.Workflow.Core.Generator;

namespace OptimaJet.Workflow.DbPersistence
{
    [Obsolete("Use class OptimaJet.Workflow.DbPersistence.MSSQLProvider")]
    public class DbXmlWorkflowGenerator : MSSQLProvider
    {
        public DbXmlWorkflowGenerator(string connectionStringName) : base(connectionStringName)
        {
        }
    }
}
#endif