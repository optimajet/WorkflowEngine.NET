#if !NETCOREAPP
using System;
using System.Configuration;

namespace OptimaJet.Workflow.DbPersistence
{
    [Obsolete("Use class OptimaJet.Workflow.DbPersistence.MSSQLProvider")]
    public abstract class DbProvider : MSSQLProvider
    {
        public DbProvider(string connectionString) : base (connectionString)
        {
            
        }
    }
}
#endif