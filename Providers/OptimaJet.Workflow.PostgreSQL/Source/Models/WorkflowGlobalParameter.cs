using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;


// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowGlobalParameter : DbObject<GlobalParameterEntity>
    {
        public WorkflowGlobalParameter(string schemaName, int commandTimeout) : base(schemaName, "WorkflowGlobalParameter", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Type)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Name)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Value)}
            });
        }

        public async Task<GlobalParameterEntity[]> SelectByTypeAndNameAsync(NpgsqlConnection connection, string type,
            string name = null)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE \"{nameof(GlobalParameterEntity.Type)}\" = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + $" AND \"{nameof(GlobalParameterEntity.Name)}\" = @name";
            }

            var p = new NpgsqlParameter("type", NpgsqlDbType.Varchar) {Value = type};

            if (String.IsNullOrEmpty(name))
            {
                return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name};

            return await SelectAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }

        public async Task<int> DeleteByTypeAndNameAsync(NpgsqlConnection connection, string type, string name = null)
        {
            string selectText = $"DELETE FROM {ObjectName}  WHERE \"{nameof(GlobalParameterEntity.Type)}\" = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + $" AND \"{nameof(GlobalParameterEntity.Name)}\" = @name";
            }

            var p = new NpgsqlParameter("type", NpgsqlDbType.Varchar) {Value = type};

            if (String.IsNullOrEmpty(name))
            {
                return await ExecuteCommandNonQueryAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name};

            return await ExecuteCommandNonQueryAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }
    }
}
