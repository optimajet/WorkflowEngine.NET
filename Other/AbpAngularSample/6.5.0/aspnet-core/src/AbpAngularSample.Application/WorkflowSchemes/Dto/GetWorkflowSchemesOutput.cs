using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace AbpAngularSample.WorkflowSchemes.Dto
{
    //WorkflowEngineSampleCode
    public class GetWorkflowSchemesOutput : EntityDto
    {
        public List<WorkflowSchemeDto> Schemes { get; set; }
    }
}