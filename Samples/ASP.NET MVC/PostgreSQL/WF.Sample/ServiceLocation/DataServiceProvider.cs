using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;

namespace WF.Sample.ServiceLocation
{
    public class DataServiceProvider : IDataServiceProvider
    {
        private readonly IContainer _container;

        public DataServiceProvider(IContainer container)
        {
            _container = container;
        }

        public T Get<T>()
        {
            return _container.Resolve<T>();
        }
    }
}