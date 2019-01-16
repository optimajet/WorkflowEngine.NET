using OptimaJet.Workflow.RavenDB;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.Workflow;

namespace WF.Sample.RavenDb.Helpers
{
    internal class CacheHelper<T>
    {
        public static object _lock = new object();
        private static DocumentStore Store => (WorkflowInit.Runtime.PersistenceProvider as RavenDBProvider).Store;

        private static List<T> _all = null;
        public static List<T> Cache
        {
            get
            {
                if (_all == null)
                {
                    lock (_lock)
                    {
                        if (_all == null)
                        {
                            _all = GetAll();
                        }
                    }
                }

                return _all;
            }
        }

        private static List<T> GetAll()
        {
            using (var session = Store.OpenSession())
            {
                return session.Query<T>().ToList();
            }
        }
    }
}
