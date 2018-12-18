using System.Threading.Tasks;
using Abp.Application.Services;
using Workflow.Authorization.Accounts.Dto;

namespace Workflow.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
