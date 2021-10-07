using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace AbpAngularSample.Documents.Dto
{
    //WorkflowEngineSampleCode
    [AutoMap(typeof(Document))]
    public class DocumentDto : EntityDto<int>
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public string State { get; set; }

        public string Scheme { get; set; }

        public Guid? ProcessId { get; set; }
    }
}