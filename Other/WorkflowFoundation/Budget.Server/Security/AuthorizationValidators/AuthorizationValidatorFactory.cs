using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Budget2.Server.Security.Interface.Services;
using Budget2.Server.Security.Services;

namespace Budget2.Server.Security.AuthorizationValidators
{
    public sealed class AuthorizationValidatorFactory
    {
        private WorkflowType _workflowType;

        private AuthorizationService _authorizationService;

        public AuthorizationValidatorFactory (WorkflowType workflowType, AuthorizationService authorizationService)
        {
            _workflowType = workflowType;
            _authorizationService = authorizationService;
        }

        public IAuthorizationValidator CreateValidator()
        {
            IAuthorizationValidator validator;

            if (_workflowType == WorkflowType.BillDemandWorkfow)
                validator = new BillDemandWorkflowAuthorizationValidator();
            else if (_workflowType == WorkflowType.DemandAdjustmentWorkflow)
                validator = new DemandAdjustmentWorflowAuthorizationValidator();
            else if (_workflowType == WorkflowType.DemandWorkflow)
                validator = new DemandWorkflowAuthorizationValidator();
            else
                throw new ArgumentException();

            validator.AuthorizationService = _authorizationService;
            validator.EmployeeService = _authorizationService.EmployeeService;
            validator.BillDemandBuinessService = _authorizationService.BillDemandBuinessService;
            validator.SecurityEntityService = _authorizationService.SecurityEntityService;

            return validator;
        }
    }
}
