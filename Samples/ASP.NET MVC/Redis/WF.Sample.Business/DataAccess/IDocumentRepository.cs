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
        List<Model.Document> Get(out int count, int page = 1, int pageSize = 128);
        Model.Document Get(Guid id, bool loadChildEntities = true);
        List<Model.Document> GetByIds(List<Guid> ids);
        void Delete(Guid[] ids);
        void ChangeState(Guid id, string nextState, string nextStateName);
        bool IsAuthorsBoss(Guid documentId, Guid identityId);
        IEnumerable<string> GetAuthorsBoss(Guid documentId);
    }
}
