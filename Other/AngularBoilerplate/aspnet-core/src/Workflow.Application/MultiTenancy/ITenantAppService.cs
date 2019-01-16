using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Workflow.MultiTenancy.Dto;

namespace Workflow.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

