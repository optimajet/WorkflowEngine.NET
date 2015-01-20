using System;
using System.Collections.Generic;
using System.Text;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Security.Interface.DataContracts;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeWeb;
using System.Linq.Expressions;
using System.Linq;

namespace Budget2.Server.Shell.Views
{
    public class WorkflowButtonsPresenter : Presenter<IWorkflowButtonsView>
    {

        // NOTE: Uncomment the following code if you want ObjectBuilder to inject the module controller
        //       The code will not work in the Shell module, as a module controller is not created by default
        //
        private IShellController _controller;
        public WorkflowButtonsPresenter([CreateNew] IShellController controller)
        {
            _controller = controller;
        }

        public override void OnViewLoaded()
        {
            // TODO: Implement code that will be executed every time the view loads
        }

        public override void OnViewInitialized()
        {
           if (!View.EntityId.HasValue || !View.IdentityId.HasValue)
           {
               View.DenialButtonVisible = View.DenialByTechnicalCausesButtonVisible = View.SignButtonVisible = false;
               return;
           }

            var opertions =_controller.WorkflowApi.GetListOfAllowedOperations(new ApiCommandArgument()
                                                                   {
                                                                       InstanceId = View.EntityId.Value,
                                                                       SecurityToken = View.IdentityId.Value
                                                                   });

            View.SignButtonVisible = opertions.Count(op => op == WorkflowCommandType.Sighting) > 0;

            View.DenialButtonVisible = opertions.Count(op => op == WorkflowCommandType.Denial) > 0;

            View.DenialByTechnicalCausesButtonVisible = opertions.Count(op => op == WorkflowCommandType.DenialByTechnicalCauses) > 0;
        }

        // TODO: Handle other view events and set state in the view
    }
}




