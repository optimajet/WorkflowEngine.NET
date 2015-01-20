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

        bool IsCommandSupportsInState(WorkflowState state, WorkflowCommandType commandType);

        bool IsCurrentUserAllowedToExecuteCommandInCurrentState(ServiceIdentity identity, WorkflowState state, Guid instanceId);

    }
}
