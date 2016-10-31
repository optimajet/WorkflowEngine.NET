using System;
using System.Data;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowProcessInstancePersistence
    {
        public string ParameterName { get; set; }
        public string Value { get; set; }
    }
}