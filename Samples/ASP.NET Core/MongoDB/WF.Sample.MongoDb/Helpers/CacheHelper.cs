using MongoDB.Bson;
using MongoDB.Driver;
using OptimaJet.Workflow.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.Workflow;

namespace WF.Sample.MongoDb.Helpers
{
    internal class CacheHelper<T>
    {
        public static object _lock = new object();
        private static IMongoDatabase Store => (WorkflowInit.Runtime.PersistenceProvider as MongoDBProvider).Store;

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
            var dbcoll = Store.GetCollection<T>(typeof(T).Name);
            return dbcoll.Find(new BsonDocument()).ToList();
        }
    }
}
