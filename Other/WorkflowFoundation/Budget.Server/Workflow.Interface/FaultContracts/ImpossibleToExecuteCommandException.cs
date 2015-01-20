using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Workflow.Interface.FaultContracts
{
    public class ImpossibleToExecuteCommandException : InvalidOperationException
    {
        public ImpossibleToExecuteCommandException (string message) : base (message)
        {}
    }
}
