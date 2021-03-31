using AutoMapper;
using WF.Sample.Business.Model;
using WF.Sample.Models;

namespace WF.Sample.Mapping
{
    public class SampleProfile : Profile
    {
        public SampleProfile()
        {
            CreateMap<DocumentModel, Document>()
                .ForMember(d => d.Author, o => o.MapFrom(s => new Employee {Id = s.AuthorId, Name = s.AuthorName}))
                .ForMember(d => d.Manager, o => o.MapFrom(s => s.ManagerId.HasValue ? new Employee {Id = s.ManagerId.Value, Name = s.ManagerName} : null))
                ;
        }
    }
}
