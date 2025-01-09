using System.Collections.Generic;

namespace WF.Sample.Models
{
    public class DocumentListModel<TDoc>:IPaging
        where TDoc:DocumentModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<TDoc> Docs { get; set; }
    }
}
