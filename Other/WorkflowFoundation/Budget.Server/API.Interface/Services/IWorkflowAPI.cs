using System.Collections.Generic;
using System.ServiceModel;
using Budget2.Server.API.Interface.DataContracts;
using Budget2.Server.Security.Interface.DataContracts;
using Common.WCF;

namespace Budget2.Server.API.Interface.Services
{
    [ServiceContract]
    public interface IWorkflowApi
    {
        [OperationContract]
        void Sighting(ApiCommandArgument arg);

        [OperationContract]
        void Denial(ApiCommandArgument arg);

        [OperationContract]
        void Rollback(ApiCommandArgument arg);

        [OperationContract]
        void DenialByTechnicalCauses(ApiCommandArgument arg);

        [OperationContract]
        void StartProcessing(ApiCommandArgument arg);

        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void PostingAccounting(ApiCommandArgument arg);

        [OperationContract]
        void CheckStatus(ApiCommandArgument arg);

        [OperationContract]
        void SetPaidStatus(BillDemandPaidApiCommandArgument arg);

        [OperationContract]
        void SetDenialStatus(ApiCommandArgument arg);

        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        void ExportBillDemand(ApiCommandArgument arg);

        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        IEnumerable<CommandExecutionStatus> MassExportBillDemands(ApiMassCommandEventArg arg);

        [OperationContract]
        [FaultContract(typeof (BaseFault))]
        IEnumerable<CommandExecutionStatus> MassExecuteCommand(ApiMassCommandEventArg arg);

        [OperationContract]
        List<WorkflowCommandType> GetListOfAllowedOperations(ApiCommandArgument arg);

        [OperationContract]
        List<WorkflowStateInfo> GetAvailiableWorkflowStateToSet(ApiCommandArgument arg);

        [OperationContract]
        void SetWorkflowState(SetStateApiCommandArgument arg);


        [OperationContract]
        [FaultContract(typeof(BaseFault))]
        IEnumerable<CommandExecutionStatus> MassExecuteSetState(ApiMassSetStateCommandEventArg arg);


    }

}
