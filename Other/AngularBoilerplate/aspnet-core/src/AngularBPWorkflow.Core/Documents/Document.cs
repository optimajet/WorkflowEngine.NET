using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AngularBPWorkflow.Documents
{
    //WorkflowEngineSampleCode
    [Table("AppDocuments")]
    public class Document : Entity, IHasCreationTime
    {
        public const int MaxTitleLength = 256;
        public const int MaxDescriptionLength = 64 * 1024; //64KB

        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public string State { get; set; }

        public string Scheme { get; set; }

        public Guid? ProcessId { get; set; }

        public Document()
        {
            CreationTime = DateTime.Now;
            State = "";
        }

        public Document(string title, string description = null)
            : this()
        {
            Title = title;
            Description = description;
        }
    }
}
