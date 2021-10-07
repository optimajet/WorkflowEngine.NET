using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using AbpAngularSample.WorkflowSchemes.Dto;

namespace AbpAngularSample.WorkflowSchemes
{
    //WorkflowEngineSampleCode
    public interface IWorkflowSchemeAppService : IApplicationService
    {
        Task<GetWorkflowSchemesOutput> GetSchemes();
        Task Delete(string code);
        Task<WorkflowCommandDto[]> GetAvaliableCommands(Guid processId);
        Task<bool> ExecuteCommand(Guid processId, string command);
        Task<string[]> GetStates(Guid processId);
        Task<bool> SetState(Guid processId, string state);
        Task<WorkflowHistoryDto[]> GetHistories(Guid processId);
    }
}