using System.Threading.Tasks;
using Abp.Application.Services;
using AngularBPWorkflow.Authorization.Accounts.Dto;

namespace AngularBPWorkflow.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
