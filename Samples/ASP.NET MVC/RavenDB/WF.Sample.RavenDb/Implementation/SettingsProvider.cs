using OptimaJet.Workflow.RavenDB;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Business.Workflow;
using WF.Sample.RavenDb.Helpers;

namespace WF.Sample.RavenDb.Implementation
{
    public class SettingsProvider : ISettingsProvider
    {
        private static DocumentStore Store => (WorkflowInit.Runtime.PersistenceProvider as RavenDBProvider).Store;

        public Settings GetSettings()
        {
            var model = new Settings
            {
                Employees = CacheHelper<Entities.Employee>.Cache.Select(x => Mappings.Mapper.Map<Employee>(x)).ToList(),
                Roles = CacheHelper<Role>.Cache,
                StructDivision = CacheHelper<StructDivision>.Cache
            };

            return model;
        }
    }
}
