using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;

namespace AngularBPWorkflow.WorkflowSchemes.Dto
{
    //WorkflowEngineSampleCode
    public class GetWorkflowSchemesOutput : EntityDto
    {
        public List<WorkflowSchemeDto> Schemes { get; set; }
    }
}
