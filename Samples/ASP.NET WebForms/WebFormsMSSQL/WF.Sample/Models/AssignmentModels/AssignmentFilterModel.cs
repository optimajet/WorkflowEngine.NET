
namespace WF.Sample.Models
{
    public class AssignmentFilterModel
    {
        public int Page { get; set; } = 1;

        public string FilterName { get; set; } = "Active and assigned to me";
        
        public int? DocumentNumber{ get; set; }
        
        public string AssignmentCode { get; set; }

        public string StatusState { get; set; } = "Any";
    }
}
