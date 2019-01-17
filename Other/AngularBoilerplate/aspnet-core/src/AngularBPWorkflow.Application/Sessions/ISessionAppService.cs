using System.Threading.Tasks;
using Abp.Application.Services;
using AngularBPWorkflow.Sessions.Dto;

namespace AngularBPWorkflow.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
