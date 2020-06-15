using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Zero.Configuration;
using AngularBPWorkflow.Authorization.Users;

namespace AngularBPWorkflow.Authorization.Roles
{
    public class RoleManager : AbpRoleManager<Role, User>
    {
        public RoleManager(
            RoleStore store, 
            IEnumerable<IRoleValidator<Role>> roleValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            ILogger<AbpRoleManager<Role, User>> logger,
            IPermissionManager permissionManager, 
            ICacheManager cacheManager, 
            IUnitOfWorkManager unitOfWorkManager,
            IRoleManagementConfig roleManagementConfig)
            : base(
                  store,
                  roleValidators, 
                  keyNormalizer, 
                  errors, logger, 
                  permissionManager,
                  cacheManager, 
                  unitOfWorkManager,
                  roleManagementConfig)
        {
        }
    }
}
