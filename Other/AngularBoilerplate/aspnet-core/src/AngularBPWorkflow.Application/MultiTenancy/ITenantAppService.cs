using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AngularBPWorkflow.MultiTenancy.Dto;

namespace AngularBPWorkflow.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

