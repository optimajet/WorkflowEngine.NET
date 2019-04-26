using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Business.DataAccess
{
    public interface IDocumentRepository
    {
        Model.Document InsertOrUpdate(Model.Document doc);
        void DeleteEmptyPreHistory(Guid processId);
        List<Model.Document> Get(out int count, int page = 0, int pageSize = 128);
        List<Model.Document> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.Document> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.DocumentTransitionHistory> GetHistory(Guid id);
        Model.Document Get(Guid id, bool loadChildEntities = true);
        void Delete(Guid[] ids);
        void ChangeState(Guid id, string nextState, string nextStateName);
        bool IsAuthorsBoss(Guid documentId, Guid identityId);
        IEnumerable<string> GetAuthorsBoss(Guid documentId);
        void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities);
        void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId);
    }
}
