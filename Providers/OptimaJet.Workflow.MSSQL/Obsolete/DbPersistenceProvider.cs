#if !NETCOREAPP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.DbPersistence
{
    [Obsolete("Use class OptimaJet.Workflow.DbPersistence.MSSQLProvider")]
    public sealed class DbPersistenceProvider : MSSQLProvider
    {
        public DbPersistenceProvider(string connectionString) : base(connectionString)
        {
        }
    }
}
#endif