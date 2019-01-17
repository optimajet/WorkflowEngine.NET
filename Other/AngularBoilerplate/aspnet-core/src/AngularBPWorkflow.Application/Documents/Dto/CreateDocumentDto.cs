using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace AngularBPWorkflow.Documents.Dto
{
    //WorkflowEngineSampleCode
    [AutoMapTo(typeof(Document))]
    public class CreateDocumentDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public string State { get; set; }

        public string Scheme { get; set; }

    }
}
