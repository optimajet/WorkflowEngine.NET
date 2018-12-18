using System.Threading.Tasks;
using Abp.Application.Services;
using Workflow.Sessions.Dto;

namespace Workflow.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
