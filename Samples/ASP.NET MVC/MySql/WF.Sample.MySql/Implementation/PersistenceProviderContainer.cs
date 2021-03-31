using OptimaJet.Workflow.Core.Persistence;
using System.Configuration;
using OptimaJet.Workflow.MySQL;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.MySql.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            Provider = new MySQLProvider(connectionString);
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
