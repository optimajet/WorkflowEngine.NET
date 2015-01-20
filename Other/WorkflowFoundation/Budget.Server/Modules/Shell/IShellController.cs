using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.API.Interface.Services;
using Budget2.Server.Business.Interface.Services;

namespace Budget2.Server.Shell
{
    public interface IShellController
    {
        IWorkflowApi WorkflowApi { get; set; }
        IWorkflowTicketService WorkflowTicketService { get; set; }
        IWorkflowStateService WorkflowStateService {get;set;}
    }
}
