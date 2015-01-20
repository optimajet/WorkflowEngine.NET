using System;
using System.Collections.Generic;
using System.Text;

namespace Budget2.Server.Shell.Views
{
    public interface IWorkflowFormView
    {
        Guid? EntityId { get; set; }
        void ShowPrintForm(Guid entityId);
        Guid? IdentityId { get; set; }
        bool ISAccessGranted { set; }
        Guid? TicketId { get; }
        void ShowError(string error);
        string Comment { get; set; }
        void ShowCompleted();
        void ShowUsed();
    }
}




