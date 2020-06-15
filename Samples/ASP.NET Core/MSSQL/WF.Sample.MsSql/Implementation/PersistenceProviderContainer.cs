using Microsoft.Extensions.Configuration;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.DbPersistence;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
