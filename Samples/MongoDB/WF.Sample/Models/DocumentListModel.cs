using System.Collections.Generic;
using WF.Sample.Business.Models;

namespace WF.Sample.Models
{
    public class DocumentListModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<Document> Docs { get; set; }
    }
}