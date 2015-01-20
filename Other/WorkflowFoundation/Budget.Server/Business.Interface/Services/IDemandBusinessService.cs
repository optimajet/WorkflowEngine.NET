using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL;
using Budget2.DAL.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IDemandBusinessService
    {
        bool CheckInitiatorIsExecutorStructDivision(Guid demandUid);
        void UpdateDemandState(WorkflowState state, Guid demandId);
        void UpdateDemandState(WorkflowState initialState, WorkflowState destinationState, WorkflowCommand command, Guid demandId,
                               Guid initiatorId, string comment);

        void CreateDemandPreHistory(Guid demandUid, WorkflowState state);
        Demand GetDemand(Guid demandId);
    }
}
