using Abp.Authorization;
using Workflow.Authorization.Roles;
using Workflow.Authorization.Users;

namespace Workflow.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
