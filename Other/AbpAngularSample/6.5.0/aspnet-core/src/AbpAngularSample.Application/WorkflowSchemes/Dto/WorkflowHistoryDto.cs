using System;

namespace AbpAngularSample.WorkflowSchemes.Dto
{
    //WorkflowEngineSampleCode
    public class WorkflowHistoryDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public string TriggerName { get; set; }
        public DateTime? TransitionTime { get; set; }
        public string TransitionClassifier { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string ExecutorIdentityName { get; set; }
    }
}