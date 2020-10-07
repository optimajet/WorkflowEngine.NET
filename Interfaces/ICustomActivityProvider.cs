using System;
using System.Collections.Generic;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface ICustomActivityProvider
    {
        /// <summary>
        /// Returns custom activities
        /// </summary>
        /// <returns></returns>
        List<ActivityBase> GetCustomActivities();
    }
}
