using System;
using System.Collections.Generic;
using System.Text;

namespace Budget2.Server.Shell.Views
{
    public interface IWorkflowButtonsView
    {
        bool SignButtonVisible { get; set; }
        bool DenialButtonVisible { get; set; }
        bool DenialByTechnicalCausesButtonVisible { get; set; }

        Guid? EntityId { get; set; }

        Guid? IdentityId { get; set; }

        event EventHandler<EventArgs> OnSignClick;
        event EventHandler<EventArgs> OnDenialClick;
        event EventHandler<EventArgs> OnDenialByTechnicalCausesClick;
    }
}




