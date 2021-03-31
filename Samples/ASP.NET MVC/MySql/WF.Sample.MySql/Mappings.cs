using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;

namespace WF.Sample.MySql
{
    internal static class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<StructDivision, Business.Model.StructDivision>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => new Guid(s.Id)))
                    .ForMember(d => d.ParentId,
                        o => o.MapFrom(s => s.ParentId == null ? null : new Guid?(new Guid(s.ParentId))));
                cfg.CreateMap<Employee, Business.Model.Employee>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => new Guid(s.Id)))
                    .ForMember(d => d.StructDivisionId, o => o.MapFrom(s => new Guid(s.StructDivisionId)));

                cfg.CreateMap<Role, Business.Model.Role>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => new Guid(s.Id)));

                cfg.CreateMap<EmployeeRole, Business.Model.EmployeeRole>()
                    .ForMember(d => d.EmployeeId, o => o.MapFrom(s => new Guid(s.EmployeeId)))
                    .ForMember(d => d.RoleId, o => o.MapFrom(s => new Guid(s.RoleId)));


                cfg.CreateMap<Document, Business.Model.Document>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => new Guid(s.Id)))
                    .ForMember(d => d.ManagerId,
                        o => o.MapFrom(s => s.ManagerId == null ? null : new Guid?(new Guid(s.ManagerId))))
                    .ForMember(d => d.AuthorId, o => o.MapFrom(s => new Guid(s.AuthorId)));

                //
                // cfg.CreateMap<StructDivision, Business.Model.StructDivision>();
                // cfg.CreateMap<Employee, Business.Model.Employee>();
                // cfg.CreateMap<Role, Business.Model.Role>();
                // cfg.CreateMap<EmployeeRole, Business.Model.EmployeeRole>();
                //
                // cfg.CreateMap<Document, Business.Model.Document>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
