#if !NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.DbPersistence
{
    [Obsolete("Use class OptimaJet.Workflow.DbPersistence.MSSQLProvider")]
    public sealed class DbSchemePersistenceProvider : MSSQLProvider
    {
        public DbSchemePersistenceProvider(string connectionStringName) : base(connectionStringName)
        {
        }
    }
}
#endif
