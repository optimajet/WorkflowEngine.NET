using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.Redis.Implementation
{
    public class SettingsProvider : RepositoryBase, ISettingsProvider
    {
        private readonly ConnectionSettingsProvider _settings;

        public SettingsProvider(ConnectionSettingsProvider settings) : base(settings)
        {
            _settings = settings;
        }

        public Settings GetSettings()
        {
            var db = _connector.GetDatabase();

            var roles = db.SetMembers(GetKeyForRolesSet());

            var divisionKeys = db.SetMembers(GetKeyForStructDivisionsSet());

            var divisions = db.StringGet(divisionKeys.Select(k => (RedisKey)GetKeyForStructDivision(new Guid((string)k))).ToArray());

            var model = new Settings
            {
                Employees = new EmployeeRepository(_settings).GetAll(db),
                Roles = roles.Select(r => new Role() { Name = r.ToString() }).ToList(),
                StructDivision = divisions.Select(d => 
                    Mappings.Mapper.Map<StructDivision>(JsonConvert.DeserializeObject<Entities.StructDivision>(d))
                ).ToList()
            };

            return model;
        }
    }
}
