﻿using Microsoft.Extensions.Configuration;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OptimaJet.Workflow.MySQL;
using WF.Sample.Business.DataAccess;

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
