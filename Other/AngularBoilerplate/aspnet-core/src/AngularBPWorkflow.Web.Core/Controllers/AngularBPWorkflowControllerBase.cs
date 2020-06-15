using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;
using OptimaJet.Workflow.Core.Runtime;

namespace AngularBPWorkflow.Controllers
{
    public abstract class AngularBPWorkflowControllerBase: AbpController
    {
        protected AngularBPWorkflowControllerBase()
        {
            LocalizationSourceName = AngularBPWorkflowConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        //WorkflowEngineSampleCode
        protected WorkflowRuntime CurrentWorkflowRuntime
        {
            get
            {
                return WorkflowManager.Runtime;
            }
        }
    }
}
