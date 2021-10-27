using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MongoDB
{
    public class WorkflowProcessInstance : DynamicEntity
    {
      
        public string ActivityName { get; set; }
        public Guid Id { get; set; }
        public bool IsDeterminingParametersChanged { get; set; }
        public string PreviousActivity { get; set; }
        public string PreviousActivityForDirect { get; set; }
        public string PreviousActivityForReverse { get; set; }
        public string PreviousState { get; set; }
        public string PreviousStateForDirect { get; set; }
        public string PreviousStateForReverse { get; set; }
        public Guid? SchemeId { get; set; }
        public string StateName { get; set; }
        public List<WorkflowProcessInstancePersistence> Persistence { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string TenantId { get; set; }
        
        public string SubprocessName { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? LastTransitionDate { get; set; }

        public WorkflowProcessInstanceStatus Status { get; set; }
        
        
        public static readonly Dictionary<string, IOrderBy> OrderFunctions =
            new Dictionary<string, IOrderBy>
            {
                { nameof(Id), new OrderBy<WorkflowProcessInstance,Guid>(x => x.Id) },
                { nameof(ActivityName), new OrderBy<WorkflowProcessInstance,string>(x => x.ActivityName) },
                { nameof(IsDeterminingParametersChanged), new OrderBy<WorkflowProcessInstance,bool>(x => x.IsDeterminingParametersChanged) },
                { nameof(PreviousActivity), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousActivity) },
                { nameof(PreviousActivityForDirect), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousActivityForDirect) },
                { nameof(PreviousActivityForReverse), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousActivityForReverse) },
                { nameof(PreviousState), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousState) },
                { nameof(PreviousStateForDirect), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousStateForDirect) },
                { nameof(PreviousStateForReverse), new OrderBy<WorkflowProcessInstance,string>(x => x.PreviousStateForReverse) },
                { nameof(SchemeId), new OrderBy<WorkflowProcessInstance,Guid?>(x => x.SchemeId) },
                { nameof(ParentProcessId), new OrderBy<WorkflowProcessInstance,Guid?>(x => x.ParentProcessId) },
                { nameof(RootProcessId), new OrderBy<WorkflowProcessInstance,Guid>(x => x.RootProcessId) },
                { nameof(TenantId), new OrderBy<WorkflowProcessInstance,string>(x => x.TenantId) },
                { nameof(SubprocessName), new OrderBy<WorkflowProcessInstance,string>(x => x.SubprocessName) },
                { nameof(CreationDate), new OrderBy<WorkflowProcessInstance,DateTime>(x => x.CreationDate) },
                { nameof( LastTransitionDate), new OrderBy<WorkflowProcessInstance,DateTime?>(x => x. LastTransitionDate) },
            };
    }
}
