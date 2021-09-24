using System.Threading.Tasks;
using Abp.Application.Services;
using AbpAngularSample.Authorization.Accounts.Dto;

namespace AbpAngularSample.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
