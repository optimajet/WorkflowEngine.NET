using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.MongoDB.Models
{
    public class WorkflowProcessAssignment : DynamicEntity
    {
        public Guid Id { get; set; }
        public string AssignmentCode { get; set; }
        public string Name { get; set; }
        public Guid ProcessId { get; set; }
        public string Description { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateFinish { get; set; }
        public DateTime? DeadlineToStart { get; set; }
        public DateTime? DeadlineToComplete { get; set; }
        public string Executor { get; set; }
        public List<string> Observers { get; set; }
        public List<string> Tags { get; set; }
        public string StatusState { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        
        public static readonly Dictionary<string, IOrderBy> OrderFunctions =
            new Dictionary<string, IOrderBy>
            {
                { "AssignmentId", new OrderBy<WorkflowProcessAssignment,Guid>(x => x.Id) },
                { nameof(AssignmentCode), new OrderBy<WorkflowProcessAssignment,string>(x => x.AssignmentCode) },
                { nameof(Name), new OrderBy<WorkflowProcessAssignment,string>(x => x.Name) },
                { nameof(ProcessId), new OrderBy<WorkflowProcessAssignment,Guid>(x => x.ProcessId) },
                { nameof(Description), new OrderBy<WorkflowProcessAssignment,string>(x => x.Description) },
                { nameof(DateCreation), new OrderBy<WorkflowProcessAssignment,DateTime>(x => x.DateCreation) },
                { nameof(DateStart), new OrderBy<WorkflowProcessAssignment,DateTime?>(x => x.DateStart) },
                { nameof(DateFinish), new OrderBy<WorkflowProcessAssignment,DateTime?>(x => x.DateFinish) },
                { nameof(DeadlineToStart), new OrderBy<WorkflowProcessAssignment,DateTime?>(x => x.DeadlineToStart) },
                { nameof(DeadlineToComplete), new OrderBy<WorkflowProcessAssignment,DateTime?>(x => x.DeadlineToComplete) },
                { nameof(Executor), new OrderBy<WorkflowProcessAssignment,string>(x => x.Executor) },
                { nameof(StatusState), new OrderBy<WorkflowProcessAssignment,string>(x => x.StatusState) },
                { nameof(IsActive), new OrderBy<WorkflowProcessAssignment,bool>(x => x.IsActive) },
                { nameof(IsDeleted), new OrderBy<WorkflowProcessAssignment,bool>(x => x.IsDeleted) },
            };
        
        public Assignment ConvertToAssignment(Core.Runtime.WorkflowRuntime runtime)
        {
            return new Assignment()
            {
                AssignmentId = Id,
                AssignmentCode = AssignmentCode,
                Name = Name,
                ProcessId = ProcessId,
                StatusState = StatusState,
                IsDeleted = IsDeleted,
                IsActive = IsActive,
                DateCreation = runtime.ToRuntimeTime(DateCreation),
                DateFinish = runtime.ToRuntimeTime(DateFinish),
                DateStart = runtime.ToRuntimeTime(DateStart),
                DeadlineToStart =  runtime.ToRuntimeTime(DeadlineToStart),
                DeadlineToComplete = runtime.ToRuntimeTime(DeadlineToComplete),
                Description = Description,
                Executor = Executor,
                Tags = Tags,
                Observers = Observers
            };
        }

        public static string GetPropertyName(string key)
        {
            return key switch
            {
                "AssignmentId" => nameof(Id),
                nameof(AssignmentCode) => nameof(AssignmentCode),
                nameof(ProcessId) => nameof(ProcessId),
                nameof(Description) => nameof(Description),
                nameof(DateCreation) => nameof(DateCreation),
                nameof(DateStart) => nameof(DateStart),
                nameof(DateFinish) => nameof(DateFinish),
                nameof(Name) => nameof(Name),
                nameof(DeadlineToStart) => nameof(DeadlineToStart),
                nameof(DeadlineToComplete) => nameof(DeadlineToComplete),
                nameof(Executor) => nameof(Executor),
                nameof(Observers) => nameof(Observers),
                nameof(Tags) => nameof(Tags),
                nameof(IsDeleted) => nameof(IsDeleted),
                nameof(IsActive) => nameof(IsActive),
                nameof(StatusState) => nameof(StatusState),
                _ => throw new Exception(string.Format("Key {0} is not exists", key))
            };
        }
    }
}
