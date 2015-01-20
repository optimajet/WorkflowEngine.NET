using System;
using System.Collections.Generic;
using Budget2.DAL.DataContracts;
using Budget2.Server.Security.Interface.DataContracts;

namespace Budget2.Server.Security.Interface.Services
{
    public interface IAuthorizationService
    {
        bool IsAllowedToExecuteCommand(Guid instanceUid, WorkflowCommandType commandType);

        bool IsAllowedToExecuteCommand(IEnumerable<Guid> instanceIds, WorkflowCommandType commandType);

        List<WorkflowCommandType> IsAllowedToExecuteCommand(Guid instanceUid,List<WorkflowCommandType> commandsToCheck);

        bool IsInRole(Guid identityId, BudgetRole role);
    }
}
