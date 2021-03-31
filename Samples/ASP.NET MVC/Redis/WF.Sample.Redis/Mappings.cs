using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Redis.Entities;

namespace WF.Sample.Redis
{
    internal class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<Document, Business.Model.Document>()
                   .ForMember(d => d.Author, o => o.MapFrom(s => new Business.Model.Employee { Id = s.AuthorId, Name = s.AuthorName }))
                   .ForMember(d => d.Manager, o => o.MapFrom(s => s.ManagerId.HasValue ?
                        new Business.Model.Employee { Id = s.ManagerId.Value, Name = s.ManagerName } :
                        null));
                
                cfg.CreateMap<StructDivision, Business.Model.StructDivision>();

                cfg.CreateMap<Employee, Business.Model.Employee>()
                   .ForMember(d => d.EmployeeRoles, o => o.MapFrom(s => s.Roles.Select(kvp => new Business.Model.EmployeeRole
                   {
                       EmployeeId = s.Id,
                       RoleId = kvp.Key,
                       Role = new Business.Model.Role
                       {
                           Id = kvp.Key,
                           Name = kvp.Value
                       }
                   })))
                   .ForMember(d => d.StructDivision, o => o.MapFrom(s => new Business.Model.StructDivision { Id = s.StructDivisionId, Name = s.StructDivisionName }))
               ;
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
