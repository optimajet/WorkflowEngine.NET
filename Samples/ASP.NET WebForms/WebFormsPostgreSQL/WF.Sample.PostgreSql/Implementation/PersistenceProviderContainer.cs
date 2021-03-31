using OptimaJet.Workflow.Core.Persistence;
using System.Configuration;
using OptimaJet.Workflow.PostgreSQL;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.PostgreSql.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            Provider = new PostgreSQLProvider(connectionString);
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
