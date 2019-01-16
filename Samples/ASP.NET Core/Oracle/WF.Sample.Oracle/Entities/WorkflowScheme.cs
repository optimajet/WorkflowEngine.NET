namespace WF.Sample.Oracle
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WORKFLOWSCHEME")]
    public partial class WorkflowScheme
    {
        [Key]
        [StringLength(256)]
        public string Code { get; set; }

        [Required]
        public string Scheme { get; set; }
    }
}
