
using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularBPWorkflow.EntityFramework
{
    [Table("WorkflowScheme")]
    public class WorkflowScheme : Entity
    {
        public virtual string Name { get; set; }
        
        public WorkflowScheme()
        {
           
        }
    }
}
