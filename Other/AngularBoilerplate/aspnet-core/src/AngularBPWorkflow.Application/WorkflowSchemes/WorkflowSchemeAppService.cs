using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using AngularBPWorkflow.WorkflowSchemes.Dto;
using AutoMapper;
using OptimaJet.Workflow.DbPersistence;

namespace AngularBPWorkflow.WorkflowSchemes
{
    //WorkflowEngineSampleCode
    [AbpAuthorize]
    public class WorkflowSchemeAppService : AngularBPWorkflowAppServiceBase, IWorkflowSchemeAppService
    {
        public WorkflowSchemeAppService()
        {

        }

        public async Task Delete(string code)
        {
            var provider = (MSSQLProvider)WorkflowManager.Runtime.PersistenceProvider;
            using (SqlConnection connection = new SqlConnection(provider.ConnectionString))
            {
                var tmp = new WorkflowScheme();
                await WorkflowScheme.DeleteAsync(connection, code);
            }
        }

        public async Task<bool> ExecuteCommand(Guid processId, string commandName)
        {
            var identityId = AbpSession.UserId?.ToString();
            var commands = await WorkflowManager.Runtime.GetAvailableCommandsAsync(processId, new List<string> { identityId }, commandName);
            var command = commands.FirstOrDefault();

            if(command != null)
            {
                var res = await WorkflowManager.Runtime.ExecuteCommandAsync(command, identityId, null);
                return res.WasExecuted;
            }

            return false;
        }

        public async Task<WorkflowCommandDto[]> GetAvaliableCommands(Guid processId)
        {
            var identityId = AbpSession.UserId?.ToString();
            var commands = await WorkflowManager.Runtime.GetAvailableCommandsAsync(processId, new List<string> { identityId });
            return commands.OrderBy(c=>c.Classifier).Select(c => new WorkflowCommandDto()
            {
                Name = c.CommandName,
                LocalizedName = c.LocalizedName,
                Classifier = c.Classifier.ToString()
            }).ToArray();
        }

        public async Task<string[]> GetStates(Guid processId)
        {
            var identityId = AbpSession.UserId?.ToString();
            var states = await WorkflowManager.Runtime.GetAvailableStateToSetAsync(processId);
            return states.Select(c => c.Name).ToArray();
        }

        public async Task<bool> SetState(Guid processId, string state)
        {
            var identityId = AbpSession.UserId?.ToString();
            await WorkflowManager.Runtime.SetStateAsync(processId, identityId, null, state);
            return true;
        }

        public async Task<GetWorkflowSchemesOutput> GetSchemes()
        {
            List<WorkflowSchemeDto> schemes = new List<WorkflowSchemeDto>();

            var provider = (MSSQLProvider)WorkflowManager.Runtime.PersistenceProvider;
            using (SqlConnection connection = new SqlConnection(provider.ConnectionString))
            {
                var tmp = new WorkflowScheme();
                var items = await WorkflowScheme.SelectAsync(connection,
                    string.Format("SELECT * FROM {0}", WorkflowScheme.ObjectName));

                foreach (var item in items.OrderBy(c=> c.Code))
                {
                    schemes.Add(new WorkflowSchemeDto()
                    {
                        Code = item.Code
                    });
                }
            }

            return new GetWorkflowSchemesOutput
            {
                Schemes = schemes
            };
        }

        public async Task<WorkflowHistoryDto[]> GetHistories(Guid processId)
        {
            var h = await WorkflowManager.Runtime.GetProcessHistoryAsync(processId);
            var res = h.Select(c => new WorkflowHistoryDto()
            {
                TransitionTime = c.TransitionTime,
                From = c.FromStateName ?? c.FromActivityName,
                To = c.ToStateName ?? c.ToActivityName,
                TriggerName = c.TriggerName,
                TransitionClassifier = c.TransitionClassifier.ToString(),
                ExecutorIdentityId = c.ExecutorIdentityId,
                ExecutorIdentityName = UserManager.Users.Where(user => user.Id.ToString() == c.ExecutorIdentityId).FirstOrDefault()?.Name
            });

            return res.ToArray();
        }
    }
}

