namespace WF.Sample.MySql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WorkflowInbox")]
    public partial class WorkflowInbox
    {
        public byte[] Id { get; set; }

        [Required]
        public byte[] ProcessId { get; set; }

        [Required]
        [StringLength(256)]
        public string IdentityId { get; set; }
    }
}
