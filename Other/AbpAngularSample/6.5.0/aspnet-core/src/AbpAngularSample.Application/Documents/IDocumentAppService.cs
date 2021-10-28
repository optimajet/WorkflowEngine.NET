using System.Threading.Tasks;
using Abp.Application.Services;
using AbpAngularSample.Documents.Dto;

namespace AbpAngularSample.Documents
{
    //WorkflowEngineSampleCode
    public interface IDocumentAppService : IAsyncCrudAppService<DocumentDto, int, PagedDocumentResultRequestDto, CreateDocumentDto, DocumentDto>
    {
        Task<SchemeForDocumentOutput[]> GetSchemes();        
    }
}