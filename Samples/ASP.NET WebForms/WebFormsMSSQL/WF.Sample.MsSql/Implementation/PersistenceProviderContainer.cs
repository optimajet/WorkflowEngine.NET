using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.DbPersistence;
using System.Configuration;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.MsSql.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            Provider = new MSSQLProvider(connectionString);
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
