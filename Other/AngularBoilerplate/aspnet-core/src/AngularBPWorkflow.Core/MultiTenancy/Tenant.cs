using Abp.MultiTenancy;
using AngularBPWorkflow.Authorization.Users;

namespace AngularBPWorkflow.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
