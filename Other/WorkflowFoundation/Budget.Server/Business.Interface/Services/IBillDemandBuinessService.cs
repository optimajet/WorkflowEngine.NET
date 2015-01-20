using System;
using System.Collections.Generic;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IBillDemandBuinessService
    {
        void UpdateBillDemandState(WorkflowState state, Guid billDemandUid);

        void UpdateBillDemandState(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid billDemandUid, Guid initiatorId, Guid? impesonatedIdentityId, string comment);

        bool LimitExecutorSight(Guid billDemandUid, Guid sighterId, Guid initiatorId);
        bool LimitManagerSight(Guid billDemandUid, Guid sighterId, Guid initiatorId);
        void LimitExecutorResetSights(Guid billDemandUid);
        void LimitManagerResetSights(Guid billDemandUid);
        decimal GetBillDemandValue(Guid billDemandUid);
        bool CheckInitiatorHeadMustSign(Guid billDemandUid);
        bool CheckInitiatorIsHead(Guid billDemandUid);
        BillDemandForExport GetBillDemandForExport(Guid billDemandUid);
        void CreateBillDemandPreHistory(Guid billDemandUid, WorkflowState state);
        void SetAllocationDate(Guid billDemandUid);
        void SetTransferDate(Guid billDemandUid);
        void SetTransferDateAndDateOfPerformance(Guid billDemandUid);
        void SetExternalParameters(Guid billDemandUid, BillDemandExternalState externalState);
        PaymentKind GetBillDemandPaymentKind(Guid billDemandUid);
        Guid GetBillDemandInitiatorId(Guid billDemandUid);
        bool CheckUPKZHeadMustSight(Guid billDemandUid);
        void DeleteDemandPermissions(Guid billDemandUid);
        BillDemand GetBillDemand(Guid billDemandUid);
        bool IsBillDemandFilialSupportExport(Guid billDemandUid);
        List<LimitSighter> GetLimitManagerSighters(Guid billDemandUid);
        List<LimitSighter> GetLimitManagerSightings(Guid billDemandUid);
        bool CheckPaymentPlanFilled(Guid billDemandUid);
        
    }
}