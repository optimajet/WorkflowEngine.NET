using System;
using System.Collections.Generic;
using System.Text;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeWeb;

namespace Budget2.Server.Shell.Views
{
    public class WorkflowFormPresenter : Presenter<IWorkflowFormView>
    {

        // NOTE: Uncomment the following code if you want ObjectBuilder to inject the module controller
        //       The code will not work in the Shell module, as a module controller is not created by default
        //
        private IShellController _controller;
        public WorkflowFormPresenter([CreateNew] IShellController controller)
        {
            _controller = controller;
        }

        public override void OnViewLoaded()
        {
            var tid = View.TicketId;
            if (!tid.HasValue)
            {
                View.ISAccessGranted = false;
                return;
            }

            TicketInfo info = _controller.WorkflowTicketService.GetTicketInfo(tid.Value);

            if (info == null || info.IsUsed)
            {
                View.ISAccessGranted = false;
                return;
            }

            var state = _controller.WorkflowStateService.GetWorkflowState(info.EntityId);

            if (state.WorkflowStateName != info.ValidStateName)
            {
                View.ShowUsed();
                return;
            }

            View.ISAccessGranted = true;
            View.ShowPrintForm(info.EntityId);
            View.EntityId = info.EntityId;
            View.IdentityId = info.IdentityId;
        }

        public void RaiseSign()
        {
            if (!View.EntityId.HasValue || !View.IdentityId.HasValue)
            {
                View.ISAccessGranted = false;
                return;
            }

            try
            {
                _controller.WorkflowApi.Sighting(GetArgument());
                View.ShowCompleted();
                _controller.WorkflowTicketService.SetTicketCompleted(View.TicketId.Value);
            }
            catch (Exception ex)
            {
                View.ShowError(ex.Message);
            }

        }

        public void RaiseDenial()
        {
            Denial();
        }

        private void Denial(bool isTechnical = false)
        {
            if (!View.EntityId.HasValue || !View.IdentityId.HasValue)
            {
                View.ISAccessGranted = false;
                return;
            }

            if (string.IsNullOrEmpty(View.Comment))
            {
                View.ShowError("Не заполнено поле коментарий!");
                return;
            }

            try
            {
                if (isTechnical)
                    _controller.WorkflowApi.DenialByTechnicalCauses(GetArgument());
                else
                    _controller.WorkflowApi.Denial(GetArgument());
                View.ShowCompleted();
                _controller.WorkflowTicketService.SetTicketCompleted(View.TicketId.Value);
            }
            catch (Exception ex)
            {
                View.ShowError(ex.Message);
            }
        }

        private ApiCommandArgument GetArgument()
        {
            return new ApiCommandArgument() { InstanceId = View.EntityId.Value, SecurityToken = View.IdentityId.Value, Comment = View.Comment };
        }

        public void RaiseDenialByTechnicalCauses()
        {
            Denial(true);
        }

        public override void OnViewInitialized()
        {



        }
    }
}




