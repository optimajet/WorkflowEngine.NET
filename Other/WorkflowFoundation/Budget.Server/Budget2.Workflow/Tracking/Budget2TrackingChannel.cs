using System;
using System.Configuration;
using System.Workflow.Runtime.Tracking;
using Budget2.DAL;
using System.Linq;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Workflow.Tracking
{
    public class Budget2TrackingChannel : TrackingChannel
    {
        private TrackingParameters _parameters;

        public Budget2TrackingChannel(TrackingParameters parameters)
        {
            _parameters = parameters;
        }

        protected override void Send(TrackingRecord record)
        {
            var activityTrackingRecord = record as ActivityTrackingRecord;

            if (activityTrackingRecord == null)
                return;

            var type = GetWorkflowType(_parameters.WorkflowType);
            if (type.StatesToIgnoreInTracking.Count(s => s.Equals(activityTrackingRecord.QualifiedName,StringComparison.InvariantCultureIgnoreCase)) > 0)
                return;

            using (var context = new Budget2DataContext(ConfigurationManager.ConnectionStrings["default"].ConnectionString))
            {
                WorkflowTrackingHistory item = new WorkflowTrackingHistory()
                                                   {
                                                       Id = Guid.NewGuid(),
                                                       TransitionTime = DateTime.Now,
                                                       StateName = activityTrackingRecord.QualifiedName,
                                                       WorkflowId = _parameters.InstanceId,
                                                       WorkflowTypeId = type.Id

                                                   };
                context.WorkflowTrackingHistories.InsertOnSubmit(item);

                context.SubmitChanges();
            }
        }

        private WorkflowType GetWorkflowType(Type type)
        {
            if (type == typeof(BillDemand))
            {
                return WorkflowType.BillDemandWorkfow;
            }
            if (type == typeof(DemandAdjustment))
            {
                return WorkflowType.DemandAdjustmentWorkflow;
            }
            if (type == typeof(Demand))
            {
                return WorkflowType.DemandWorkflow;
            }
            throw new ArgumentException("Unknown WF type"); //TODO Typed faults
        }

        protected override void InstanceCompletedOrTerminated()
        {
           //TODO
        }
    }
}
