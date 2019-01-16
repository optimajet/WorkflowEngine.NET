using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Workflow.Controllers
{
    public abstract class WorkflowControllerBase: AbpController
    {
        protected WorkflowControllerBase()
        {
            LocalizationSourceName = WorkflowConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
