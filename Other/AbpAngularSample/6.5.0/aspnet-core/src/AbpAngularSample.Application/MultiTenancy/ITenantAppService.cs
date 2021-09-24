using Abp.Application.Services;
using AbpAngularSample.MultiTenancy.Dto;

namespace AbpAngularSample.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

