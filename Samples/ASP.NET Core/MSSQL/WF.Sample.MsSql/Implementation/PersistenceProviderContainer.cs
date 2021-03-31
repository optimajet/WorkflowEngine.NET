using Microsoft.Extensions.Configuration;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.DbPersistence;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.MsSql.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer(IConfiguration config)
        {
            Provider = new MSSQLProvider(config.GetConnectionString("DefaultConnection"));
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
