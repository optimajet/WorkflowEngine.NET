using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Extensions;
using AngularBPWorkflow.Documents.Dto;
using AngularBPWorkflow.WorkflowSchemes;
using System;
using Abp.Runtime.Session;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AngularBPWorkflow.Documents
{
    //WorkflowEngineSampleCode
    [AbpAuthorize]
    public class DocumentAppService : AsyncCrudAppService<Document, DocumentDto, int, PagedDocumentResultRequestDto, CreateDocumentDto, DocumentDto>, IDocumentAppService
    {
        private readonly IRepository<Document> _documentRepository;
        private readonly IWorkflowSchemeAppService _workflowService;

        public DocumentAppService(IRepository<Document> repository, IWorkflowSchemeAppService workflowService)
            : base(repository)
        {
            _documentRepository = repository;
            _workflowService = workflowService;

            WorkflowManager.ChangeStateFuncs.TryAdd("Document", this.ChangeState);
        }

        public override async Task<DocumentDto> Create(CreateDocumentDto input)
        {
            var document = ObjectMapper.Map<Document>(input);
            if (!string.IsNullOrEmpty(document.Scheme))
            {
                var state = await WorkflowManager.Runtime.GetInitialStateAsync(document.Scheme);
                document.State = state?.Name;
            }

            if (document.State == null)
                document.State = "None";

            document.Id = await _documentRepository.InsertAndGetIdAsync(document);

            document.ProcessId = await CreateWorkflowIfNotExists(document);
            if (document.ProcessId.HasValue)
            {
                await _documentRepository.UpdateAsync(document);
            }

            return MapToEntityDto(document);
        }

        private async Task<Guid?> CreateWorkflowIfNotExists(Document document)
        {
            if (!string.IsNullOrEmpty(document.Scheme) && document.ProcessId == null)
            {
                Guid processId = Guid.NewGuid();
                var createInstanceParams = new CreateInstanceParams(document.Scheme, processId)
                {
                    InitialProcessParameters = new Dictionary<string, object>()
                      {
                          {"objectId", document.Id},
                          {"objectType", "Document"}
                      },
                };

                await WorkflowManager.Runtime.CreateInstanceAsync(createInstanceParams);

                var pi = await WorkflowManager.Runtime.GetProcessInstanceAndFillProcessParametersAsync(processId);
                foreach(var param in createInstanceParams.InitialProcessParameters)
                {
                    pi.SetParameter(param.Key, param.Value, ParameterPurpose.Persistence);
                }
                WorkflowManager.Runtime.PersistenceProvider.SavePersistenceParameters(pi);
                return processId;
            }
            return null;
        }

        public override async Task<DocumentDto> Update(DocumentDto input)
        {
            var document = await _documentRepository.GetAsync(input.Id);
            ObjectMapper.Map(input, document);
            await _documentRepository.UpdateAsync(document);
            return MapToEntityDto(document);
        }

        private async Task ChangeState(object id, string state, string localizedState)
        {
            var document = await _documentRepository.GetAsync((int)id);
            document.State = state;
            await _documentRepository.UpdateAsync(document);
        }

        public override async Task Delete(EntityDto<int> input)
        {
            var document = await GetEntityByIdAsync(input.Id);
            if (document != null)
            {
                await _documentRepository.DeleteAsync(input.Id);

                if (document.ProcessId.HasValue)
                {
                    await WorkflowManager.Runtime.DeleteInstanceAsync(document.ProcessId.Value);
                }
            }
        }

        protected override async Task<Document> GetEntityByIdAsync(int id)
        {
            return await _documentRepository.GetAsync(id);
        }

        protected override IQueryable<Document> ApplySorting(IQueryable<Document> query, PagedDocumentResultRequestDto input)
        {
            return query.OrderBy(r => r.Title);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<SchemeForDocumentOutput[]> GetSchemes()
        {
            var schemes = await _workflowService.GetSchemes();

            var res = new List<SchemeForDocumentOutput>();
            foreach (var scheme in schemes.Schemes)
            {
                var identityId = AbpSession.UserId?.ToString();
                var commands = await WorkflowManager.Runtime.GetInitialCommandsAsync(scheme.Code, identityId);
                res.Add(new SchemeForDocumentOutput()
                {
                    Scheme = scheme.Code,
                    Commands = commands.Select(c => new SchemeForDocumentCommandOutput()
                    {
                        Name = c.CommandName,
                        LocalizedName = c.LocalizedName,
                        Classifier = c.Classifier.ToString()
                    }).ToList()
                });
            }

            return res.ToArray();
        }
    }
}

