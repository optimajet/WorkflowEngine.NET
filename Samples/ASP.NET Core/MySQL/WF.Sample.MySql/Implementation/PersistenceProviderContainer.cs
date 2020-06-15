using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.MySQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WF.Sample.Business.DataAccess;
using Microsoft.Extensions.Configuration;

namespace WF.Sample.MySql.Implementation
{
    public class PersistenceProviderContainer : IPersistenceProviderContainer
    {
        public PersistenceProviderContainer(IConfiguration config)
        {
            Provider = new MySQLProvider(config.GetConnectionString("DefaultConnection"));
        }

        public IWorkflowProvider Provider { get; private set; }
    }
}
