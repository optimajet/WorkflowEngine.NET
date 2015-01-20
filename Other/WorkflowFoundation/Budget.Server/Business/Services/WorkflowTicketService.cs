using System;
using System.Linq;
using Budget2.DAL;
using Budget2.Server.Business.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;

namespace Budget2.Server.Business.Services
{
    public class WorkflowTicketService :    Budget2DataContextService, IWorkflowTicketService
    {
        public Guid CreateTicket(Guid employeeId, Guid entityId, string stateName)
        {
            var ticketId = Guid.NewGuid();
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var ticket = new WorkflowTicket()
                                                {
                                                    EntityId = entityId,
                                                    Id = ticketId,
                                                    IsUsed = false,
                                                    ValidStateName = stateName,
                                                    ValidUntil = DateTime.MaxValue,
                                                    IdentityId = employeeId
                                                };
                    context.WorkflowTickets.InsertOnSubmit(ticket);
                    context.SubmitChanges();
                }

                scope.Complete();
                
            }

            return ticketId;
        }

        public TicketInfo GetTicketInfo(Guid ticketId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var ticket = context.WorkflowTickets.FirstOrDefault(wt => wt.Id == ticketId);
                    if (ticket == null)
                        return null;

                    return new TicketInfo()
                               {EntityId = ticket.EntityId, IdentityId = ticket.IdentityId, IsUsed = ticket.IsUsed, ValidStateName = ticket.ValidStateName};

                }
            }
        }

        public void SetTicketCompleted (Guid ticketId)
        {
            using (var scope = ReadCommittedSupressedScope)
            {
                using (var context = this.CreateContext())
                {
                    var ticket = context.WorkflowTickets.FirstOrDefault(wt => wt.Id == ticketId);
                    if (ticket == null)
                        return;

                    ticket.IsUsed = true;

                    context.SubmitChanges();

                }

                scope.Complete();
            }
        }
    }
}
