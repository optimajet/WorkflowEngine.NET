using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq;
using Abp.Organizations;
using AbpAngularSample.Authorization.Roles;

namespace AbpAngularSample.Authorization.Users
{
    public class UserStore : AbpUserStore<Role, User>
    {
        public UserStore(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, long> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<UserClaim, long> userClaimRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository) 
            : base(unitOfWorkManager,
                  userRepository,
                  roleRepository,
                  userRoleRepository,
                  userLoginRepository,
                  userClaimRepository,
                  userPermissionSettingRepository,
                  userOrganizationUnitRepository,
                  organizationUnitRoleRepository
                  )
        {
        }
    }
}
