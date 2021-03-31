using System;
using WF.Sample.Models;

namespace WF.Sample.Helpers
{
    public static class DocumentExtensions
    {
        public static TDoc ToDocumentModel<TDoc>(this Business.Model.Document d)
            where TDoc:DocumentModel, new()
        {
            return new TDoc()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Author.Name,
                Comment = d.Comment,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue ? d.Manager.Name : String.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
                Sum = d.Sum
            };
        }
    }
}
