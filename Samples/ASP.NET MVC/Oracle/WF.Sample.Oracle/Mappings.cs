using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Persistence;

namespace WF.Sample.Oracle
{
    internal static class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<StructDivision, Business.Model.StructDivision>();
                cfg.CreateMap<Employee, Business.Model.Employee>()
                    .ForMember(d => d.IsHead, o => o.MapFrom(s => s.IsHead != 0))
                    ;

                cfg.CreateMap<Role, Business.Model.Role>();
                cfg.CreateMap<EmployeeRole, Business.Model.EmployeeRole>();

                cfg.CreateMap<Document, Business.Model.Document>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
