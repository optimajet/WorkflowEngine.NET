using Abp.Application.Services.Dto;

namespace AbpAngularSample.Documents.Dto
{
    //WorkflowEngineSampleCode
    public class PagedDocumentResultRequestDto : PagedResultRequestDto
    {
        public string DocumentName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}