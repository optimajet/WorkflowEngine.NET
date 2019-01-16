using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.Model;

namespace WF.Sample.MsSql
{
    internal static class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<Business.StructDivision, StructDivision>();
                cfg.CreateMap<Business.Employee, Employee>();
                cfg.CreateMap<Business.Role, Role>();
                cfg.CreateMap<Business.EmployeeRole, EmployeeRole>();
                cfg.CreateMap<Business.DocumentTransitionHistory, DocumentTransitionHistory>();

                cfg.CreateMap<Business.Document, Document>()
                    .ForMember(d => d.Author, o => o.MapFrom(s => s.Employee1))
                    .ForMember(d => d.Manager, o => o.MapFrom(s => s.Employee));
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
