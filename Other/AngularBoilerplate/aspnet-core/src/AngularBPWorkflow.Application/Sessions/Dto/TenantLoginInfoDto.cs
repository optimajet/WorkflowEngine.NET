using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AngularBPWorkflow.MultiTenancy;

namespace AngularBPWorkflow.Sessions.Dto
{
    [AutoMapFrom(typeof(Tenant))]
    public class TenantLoginInfoDto : EntityDto
    {
        public string TenancyName { get; set; }

        public string Name { get; set; }
    }
}
