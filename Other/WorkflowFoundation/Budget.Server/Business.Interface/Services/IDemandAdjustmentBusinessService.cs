using System;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IDemandAdjustmentBusinessService
    {
        bool CheckLimitIsContingency(Guid demandAdjustmentId);
        bool CheckLimitsManagersIsSame(Guid demandAdjustmentId);
        // bool CheckLimitDistributionIsFromFutureQuoterToCurrent(Guid demandAdjustmentId);
        void UpdateDemandAdjustmentState(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command,
                                         Guid demandAdjustmentUid,
                                         Guid initiatorId, string comment);

        void UpdateDemandAdjustmentState(WorkflowState state, Guid demandAdjustmentId);
        DemandAdjustmentType GetDemandAdjustmentType(Guid demandAdjustmentId);
        bool CheckToSkipUPKZSighting(Guid demandAdjustmentId);
        bool CheckToSkipTargetDemandLimitManagerSighting(Guid demandAdjustmentId);
        bool CheckToSkipSourceDemandLimitManagerSighting(Guid demandAdjustmentId);
        bool CheckToSkipTargetDemandLimitExecutorSighting(Guid demandAdjustmentId);
        void CreateDemandAdjustmentPreHistory(Guid demandAdjustmentId, WorkflowState state);
        DemandAdjustmentSighters GetSightersId(Guid demandAdjustmentId);
        void SetStartProcessingDate(Guid demnadAdjustmentId);
    }
}
