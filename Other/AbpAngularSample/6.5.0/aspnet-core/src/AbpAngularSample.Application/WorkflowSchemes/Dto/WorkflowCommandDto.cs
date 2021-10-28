using Abp.Application.Services.Dto;

namespace AbpAngularSample.WorkflowSchemes.Dto
{
    //WorkflowEngineSampleCode
    public class WorkflowCommandDto : EntityDto
    {
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public string Classifier { get; set; }
    }
}