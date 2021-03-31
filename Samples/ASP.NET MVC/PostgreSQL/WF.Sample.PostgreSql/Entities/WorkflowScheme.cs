namespace WF.Sample.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
 

    [Table("public.WorkflowScheme")]
    public partial class WorkflowScheme
    {
        [Key]
        [StringLength(256)]
        public string Code { get; set; }

        [Required]
        public string Scheme { get; set; }
    }
}
