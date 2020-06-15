using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AngularBPWorkflow.Documents.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AngularBPWorkflow.Documents
{
    //WorkflowEngineSampleCode
    public interface IDocumentAppService : IAsyncCrudAppService<DocumentDto, int, PagedDocumentResultRequestDto, CreateDocumentDto, DocumentDto>
    {
        Task<SchemeForDocumentOutput[]> GetSchemes();        
    }
}
