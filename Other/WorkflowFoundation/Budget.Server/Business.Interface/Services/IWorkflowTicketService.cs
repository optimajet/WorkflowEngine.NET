using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IWorkflowTicketService
    {
        Guid CreateTicket(Guid employeeId, Guid entityId, string stateName);
        TicketInfo GetTicketInfo(Guid ticketId);
        void SetTicketCompleted(Guid ticketId);
    }
}
