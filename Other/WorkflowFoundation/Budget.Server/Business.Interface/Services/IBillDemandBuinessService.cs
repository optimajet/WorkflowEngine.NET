using System;
using System.Collections.Generic;
using System.Data.Linq;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public class UpdateBillDemandStateParams
    {
        private WorkflowState _initialState;
        private WorkflowState _destinationState;
        private WorkflowCommand _command;
        private Guid _billDemandUid;
        private Guid _initiatorId;
        private Guid? _impesonatedIdentityId;
        private string _comment;
        private DateTime? _sightingTime;

        public UpdateBillDemandStateParams(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid billDemandUid, Guid initiatorId, Guid? impesonatedIdentityId, string comment)
        {
            _initialState = initialState;
            _destinationState = destinationState;
            _command = command;
            _billDemandUid = billDemandUid;
            _initiatorId = initiatorId;
            _impesonatedIdentityId = impesonatedIdentityId;
            _comment = comment;
        }

        public UpdateBillDemandStateParams(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid billDemandUid, Guid initiatorId, Guid? impesonatedIdentityId, string comment, DateTime sightingTime) :
            this(initialState,destinationState,command,billDemandUid,initiatorId,impesonatedIdentityId,comment)
        {
           _sightingTime = sightingTime;
        }

        public WorkflowState InitialState
        {
            get { return _initialState; }
        }

        public WorkflowState DestinationState
        {
            get { return _destinationState; }
        }

        public WorkflowCommand Command
        {
            get { return _command; }
        }

        public Guid BillDemandUid
        {
            get { return _billDemandUid; }
        }

        public Guid InitiatorId
        {
            get { return _initiatorId; }
        }

        public Guid? ImpesonatedIdentityId
        {
            get { return _impesonatedIdentityId; }
        }

        public string Comment
        {
            get { return _comment; }
        }

        public DateTime? SightingTime
        {
            get { return _sightingTime; }
        }
    }

    public interface IBillDemandBuinessService
    {
        void UpdateBillDemandState(WorkflowState state, Guid billDemandUid);

        void UpdateBillDemandState(UpdateBillDemandStateParams updateBillDemandStateParams);
        bool LimitExecutorSight(Guid billDemandUid, Guid sighterId, Guid initiatorId);
        bool LimitManagerSight(Guid billDemandUid, Guid sighterId, Guid initiatorId, bool isOperative);
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
        bool IsBillDemandSupportExport(Guid billDemandUid);
        List<LimitSighter> GetLimitManagerSighters(Guid billDemandUid, bool IsOperative);
        List<LimitSighting> GetLimitManagerSightings(Guid billDemandUid);
        bool CheckPaymentPlanFilled(Guid billDemandUid);
        bool IsBillDemandFilialSupportsExport(Guid billDemandUid);
        bool IsBillDemandSkipInAccountingState(Guid billDemandUid);
        BillDemand GetBillDemand(Guid billDemandUid, DataLoadOptions loadWith);
        BudgetDocumentKind GetDocumentKind(Guid billDemandUid);
        bool IsOperative(Guid billDemandUid);

    }
}