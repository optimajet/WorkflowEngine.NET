using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;
using Budget2.Server.API.Interface.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IWorkflowStateService
    {
        /// <summary>
        /// Возвращает null если информация о WF не найдена в треке
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        WorkflowState GetCurrentState(Guid instanceId);

        WorkflowType TryGetExpectedWorkflowType(Guid instanceId);

        List<WorkflowStateInfo> GetAllAvailiableStates(Guid instanceId);

        WorkflowStateInfo GetWorkflowStateInfo(WorkflowState state);
        /// <summary>
        /// Если информация о WF не найдена в треке, определяет его тип и возвращает неопределенное состояние
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        WorkflowState GetWorkflowState(Guid instanceId);

        IEnumerable<Guid> GetAllWFInAction();
    }
}
