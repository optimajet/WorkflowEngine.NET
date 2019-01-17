using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;

namespace AngularBPWorkflow.WorkflowSchemes.Dto
{
    //WorkflowEngineSampleCode
    public class WorkflowCommandDto : EntityDto
    {
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public string Classifier { get; set; }
    }
}
