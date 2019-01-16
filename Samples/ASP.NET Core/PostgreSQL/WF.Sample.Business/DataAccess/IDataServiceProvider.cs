using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Business.DataAccess
{
    public interface IDataServiceProvider
    {
        T Get<T>();
    }
}
