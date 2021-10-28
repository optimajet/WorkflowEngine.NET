using System.Threading.Tasks;
using Abp.Application.Services;
using AbpAngularSample.Sessions.Dto;

namespace AbpAngularSample.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
