using System;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;

namespace AngularBPWorkflow.Documents.Dto
{
    //WorkflowEngineSampleCode
    public class DocumentListDto : EntityDto, IHasCreationTime
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }
        
        public string State { get; set; }

        public string Scheme { get; set; }

        public Guid? ProcessId { get; set; }
    }
}
