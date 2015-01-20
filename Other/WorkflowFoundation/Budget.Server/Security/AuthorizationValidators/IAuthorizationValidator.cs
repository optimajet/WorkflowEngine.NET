using System;
using System.Collections.Generic;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;

namespace Budget2.Server.Security.AuthorizationValidators
{
    public interface IAuthorizationValidator
    {
        IAuthorizationService AuthorizationService { get; set; }

        IEmployeeService EmployeeService { get; set; }

        ISecurityEntityService SecurityEntityService { get; set; }

        bool IsCommandSupportsInState(WorkflowState state, WorkflowCommandType commandType, Guid instanceId);

        bool IsCurrentUserAllowedToExecuteCommandInCurrentState(ServiceIdentity identity, WorkflowState state, Guid instanceId);

        IBillDemandBuinessService BillDemandBuinessService { get; set; }

        List<WorkflowCommandType> AddAdditionalCommand(ServiceIdentity getCurrentIdentity, IEnumerable<ServiceIdentity> identities, WorkflowState currentState, Guid instanceUid, List<WorkflowCommandType> allowedOperations);
    }
}
