using Abp.Authorization;
using AngularBPWorkflow.Authorization.Roles;
using AngularBPWorkflow.Authorization.Users;

namespace AngularBPWorkflow.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
