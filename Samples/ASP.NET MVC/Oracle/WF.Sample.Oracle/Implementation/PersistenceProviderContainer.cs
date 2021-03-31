using OptimaJet.Workflow.Core.Persistence;
using System.Configuration;
using OptimaJet.Workflow.Oracle;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.Oracle.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            Provider = new OracleProvider(connectionString);
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
