using Abp.Authorization;
using AbpAngularSample.Authorization.Roles;
using AbpAngularSample.Authorization.Users;

namespace AbpAngularSample.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
