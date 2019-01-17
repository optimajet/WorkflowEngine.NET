using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.MsSql
{
    public class SettingsProvider : ISettingsProvider
    {
      
        public Settings GetSettings()
        {
            var model = new Settings();
          
            using (var context = new Business.DataModelDataContext())
            {
                var lo = new DataLoadOptions();
                lo.LoadWith<Business.Employee>(c => c.StructDivision);
                lo.LoadWith<Business.EmployeeRole>(c => c.Role);
                lo.LoadWith<Business.Employee>(c => c.EmployeeRoles);
                context.LoadOptions = lo;

                var wfSheme = context.WorkflowSchemes.FirstOrDefault(c => c.Code == model.SchemeName);
                if (wfSheme != null)
                    model.WFSchema = wfSheme.Scheme;
                
                model.Employees = Mappings.Mapper.Map<IList<Business.Employee>, List<Employee>>(context.Employees.ToList());
                model.Roles = Mappings.Mapper.Map<IList<Business.Role>, List<Role>>(context.Roles.ToList());
                model.StructDivision = Mappings.Mapper.Map<IList<Business.StructDivision>, List<StructDivision>>(context.StructDivisions.ToList());
                return model;
            }
        }


    }
}
