using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace AngularBPWorkflow.Authorization
{
    public class AngularBPWorkflowAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, AngularBPWorkflowConsts.LocalizationSourceName);
        }
    }
}
