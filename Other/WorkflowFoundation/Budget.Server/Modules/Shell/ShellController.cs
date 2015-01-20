using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.API.Interface.Services;
using Budget2.Server.Business.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using Microsoft.Practices.ObjectBuilder;

namespace Budget2.Server.Shell
{
    public class ShellController : IShellController
    {
        [InjectionConstructor]
        public ShellController([ServiceDependency]IWorkflowApi workflowApi, [ServiceDependency]IWorkflowTicketService workflowTicketService, [ServiceDependency]IWorkflowStateService workflowStateService)
        {
            WorkflowApi = workflowApi;
            WorkflowTicketService = workflowTicketService;
            WorkflowStateService = workflowStateService;
        }

        public IWorkflowApi WorkflowApi
        {
            get;
            set;
        }

        public IWorkflowTicketService WorkflowTicketService
        {
            get;
            set;
        }

        public IWorkflowStateService WorkflowStateService
        {
            get;
            set;
        }
    }
}
