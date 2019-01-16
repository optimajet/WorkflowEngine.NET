namespace WF.Sample.MySql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WorkflowInbox")]
    public partial class WorkflowInbox
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProcessId { get; set; }

        [Required]
        [StringLength(256)]
        public string IdentityId { get; set; }
    }
}
