namespace WF.Sample.Oracle
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DOCUMENTTRANSITIONHISTORY")]
    public partial class DocumentTransitionHistory
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        public Guid? EmployeeId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string AllowedToEmployeeNames { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TransitionTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Order { get; set; }

        [Required]
        [StringLength(1024)]
        public string InitialState { get; set; }

        [Required]
        [StringLength(1024)]
        public string DestinationState { get; set; }

        [Required]
        [StringLength(1024)]
        public string Command { get; set; }

        public virtual Document Document { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
