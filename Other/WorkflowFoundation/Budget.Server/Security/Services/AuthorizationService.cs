using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Budget2.DAL;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.AuthorizationValidators;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;
using Microsoft.Practices.CompositeWeb;
using WorkflowType = Budget2.DAL.DataContracts.WorkflowType;

namespace Budget2.Server.Security.Services
{
    public class AuthorizationService : Budget2DataContextService, IAuthorizationService
    {
        [ServiceDependency]
        public IAuthenticationService AuthenticationService { get; set; }

        [ServiceDependency]
        public IWorkflowStateService WorkflowStateService { get; set; }

        [ServiceDependency]
        public ISecurityEntityService SecurityEntityService { get; set; }

        [ServiceDependency]
        public IEmployeeService EmployeeService { get; set; }

        public bool IsAllowedToExecuteCommand(Guid instanceUid, WorkflowCommandType commandType)
        {
            return IsAllowedToExecuteCommand(instanceUid, new List<WorkflowCommandType>(1) {commandType}).Count == 1;
        }

        public bool IsAllowedToExecuteCommand(IEnumerable<Guid> instanceIds, WorkflowCommandType commandType)
        {
            //STUB Сюда добавится еще и масс согласование заявок
            if (commandType != WorkflowCommandType.Export)
                return false;

            if (SecurityEntityService.CheckTrusteeWithIdIsInRole(AuthenticationService.GetCurrentIdentity().Id,BudgetRole.Accountant))
                return true;
            return false;
        }

        public List<WorkflowCommandType> IsAllowedToExecuteCommand(Guid instanceUid,
                                                                   List<WorkflowCommandType> commandsToCheck)
        {
            var allowedOperations = new List<WorkflowCommandType>(commandsToCheck.Count);

            WorkflowState currentState;

            try
            {
                currentState = WorkflowStateService.GetCurrentState(instanceUid);
            }
            catch (ArgumentException)
            {
                return allowedOperations;
            }

            WorkflowType workflowType = currentState == null
                                            ? WorkflowStateService.TryGetExpectedWorkflowType(instanceUid)
                                            : currentState.Type;

            if (workflowType == null)
                return allowedOperations;

            IAuthorizationValidator validator;

            try
            {
                var validatorFactory = new AuthorizationValidatorFactory(workflowType, this);
                validator = validatorFactory.CreateValidator();
            }
            catch (ArgumentException)
            {
                return allowedOperations;
            }

            if (!AuthorizeAccessAndImpersonateIfNecessary(validator,currentState,instanceUid))
                return allowedOperations;
    
            foreach (var commandToCheck in commandsToCheck)
            {
                if (validator.IsCommandSupportsInState(currentState, commandToCheck))
                    allowedOperations.Add(commandToCheck);
            }

            return allowedOperations;
        }

        private bool AuthorizeAccessAndImpersonateIfNecessary (IAuthorizationValidator validator, WorkflowState currentState, Guid instanceId)
        {
            var identity = AuthenticationService.GetCurrentIdentity();

            if (validator.IsCurrentUserAllowedToExecuteCommandInCurrentState(identity, currentState, instanceId))
                return true;

            var deputyId = GetSuitableDeputyId(validator, identity.Id, currentState, instanceId);

            if (!deputyId.HasValue)
                return false;

            return identity.TryImpersonate(deputyId.Value);
        }

        private Guid? GetSuitableDeputyId(IAuthorizationValidator validator, Guid userId, WorkflowState currentState, Guid instanceId)
        {
            using (var context = CreateContext())
            {
                var deputies = context.DeputyEmployees.Where(
                        p =>
                        p.StartDate <= DateTime.Today && p.EndDate >= DateTime.Today 
                        && p.DeputyEmployee1 == userId && p.Employee != userId)
                        .Select(p => p.Employee)
                        .Distinct()
                        .ToList();

                    if (deputies.Count == 0)
                        return null;

                    foreach (var deputyId in deputies)
                    {
                        if (validator.IsCurrentUserAllowedToExecuteCommandInCurrentState(new ServiceIdentity(string.Empty, deputyId, false), currentState, instanceId))
                            return deputyId;
                    }

                    return null;
            }
        }

        public bool IsInRole(Guid identityId, BudgetRole role)
        {
            return SecurityEntityService.CheckTrusteeWithIdIsInRole(identityId, role);
        }
    }
}