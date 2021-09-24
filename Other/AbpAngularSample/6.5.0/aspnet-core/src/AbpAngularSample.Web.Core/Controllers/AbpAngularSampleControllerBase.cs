using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace AbpAngularSample.Controllers
{
    public abstract class AbpAngularSampleControllerBase: AbpController
    {
        protected AbpAngularSampleControllerBase()
        {
            LocalizationSourceName = AbpAngularSampleConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
