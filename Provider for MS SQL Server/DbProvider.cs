using System.Configuration;

namespace OptimaJet.Workflow.DbPersistence
{
    public abstract class DbProvider
    {
        protected string ConnectionString { get; set; }

        public DbProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public WorkflowPersistenceModelDataContext CreateContext()
        {
            var context =
                new WorkflowPersistenceModelDataContext(ConnectionString)
                    {
                        CommandTimeout = 600, 
                        DeferredLoadingEnabled = true
                    };
            return context;
        }
    }
}
