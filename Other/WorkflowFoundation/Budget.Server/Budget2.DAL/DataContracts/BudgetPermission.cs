using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    public class BudgetPermission
    {
        public Guid Id { get; set; }

        public WorkflowType WorkflowType { get; set;}

        public WorkflowState LinkedStateToSet { get; set; }

        public static readonly BudgetPermission CanExecuteSetDraftStateForBillDemand = new BudgetPermission() 
                                                                                           { 
                                                                                               Id = new Guid("584A9A70-FA04-A28A-2EBD-EC6FF155400E"),
                                                                                               WorkflowType = DataContracts.WorkflowType.BillDemandWorkfow,
                                                                                               LinkedStateToSet = WorkflowState.BillDemandDraft
                                                                                           };
        public static readonly BudgetPermission CanExecuteSetPaidStateForBillDemand = new BudgetPermission() 
                                                                                           { 
                                                                                               Id = new Guid("26C81C59-84CE-01B8-1581-E15A155C423F"),
                                                                                               WorkflowType = DataContracts.WorkflowType.BillDemandWorkfow,
                                                                                               LinkedStateToSet = WorkflowState.BillDemandPaid
                                                                                           }; 
        public static readonly BudgetPermission CanExecuteSetDraftStateForDemand = new BudgetPermission() { 
                                                                                               Id = new Guid("584FFEB5-7BB1-E906-8341-5CED840716A0"),
                                                                                               WorkflowType = DataContracts.WorkflowType.DemandWorkflow,
                                                                                               LinkedStateToSet = WorkflowState.DemandDraft
                                                                                           }; 
        public static readonly BudgetPermission CanExecuteSetAgreedStateForDemand = new BudgetPermission() { 
                                                                                               Id = new Guid("D55E6BEE-1A11-D8B0-46EB-DB35B203C34E"),
                                                                                               WorkflowType = DataContracts.WorkflowType.DemandWorkflow,
                                                                                               LinkedStateToSet = WorkflowState.DemandAgreed
                                                                                           }; 
        public static readonly BudgetPermission CanExecuteSetDraftStateForDemandAdjustment = new BudgetPermission() {
                                                                                               Id = new Guid("FA79EBFF-5D38-A2AF-1661-F95E5E600535"),
                                                                                               WorkflowType = DataContracts.WorkflowType.DemandAdjustmentWorkflow,
                                                                                               LinkedStateToSet = WorkflowState.DemandAdjustmentDraft
                                                                                           };
        public static readonly BudgetPermission CanExecuteSetAgreedStateForDemandAdjustment = new BudgetPermission() {
                                                                                                      Id = new Guid("2D6E49FF-D58A-357D-B504-2EAA03E6F20A"),
                                                                                                      WorkflowType = DataContracts.WorkflowType.DemandAdjustmentWorkflow,
                                                                                                      LinkedStateToSet = WorkflowState.DemandAdjustmentAgreed
                                                                                                  };

        public static readonly IEnumerable<BudgetPermission> AllPermissions = new List<BudgetPermission>()
                                                                  {
                                                                      CanExecuteSetDraftStateForBillDemand,
                                                                      CanExecuteSetPaidStateForBillDemand,
                                                                      CanExecuteSetDraftStateForDemand,
                                                                      CanExecuteSetAgreedStateForDemand,
                                                                      CanExecuteSetDraftStateForDemandAdjustment,
                                                                      CanExecuteSetAgreedStateForDemandAdjustment

                                                                  };

    }
}
