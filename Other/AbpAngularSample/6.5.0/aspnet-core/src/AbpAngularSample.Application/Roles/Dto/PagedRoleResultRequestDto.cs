using Abp.Application.Services.Dto;

namespace AbpAngularSample.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

