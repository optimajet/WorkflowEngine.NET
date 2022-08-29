using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowGlobalParameter : DbObject<GlobalParameterEntity>
    {
        public WorkflowGlobalParameter(string schemaName, int commandTimeout) : base(schemaName, "WorkflowGlobalParameter", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Id), IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Type)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Name)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Value)}
            });
        }

        public async Task<GlobalParameterEntity[]> SelectByTypeAndNameAsync(SqlConnection connection, string type, string name = null)
        {
            string selectText = $"SELECT * FROM {ObjectName} WHERE [{nameof(GlobalParameterEntity.Type)}] = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + $" AND [{nameof(GlobalParameterEntity.Name)}] = @name";
            }

            var p = new SqlParameter("type", SqlDbType.NVarChar) {Value = type};

            if (String.IsNullOrEmpty(name))
            {
                return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new SqlParameter("name", SqlDbType.NVarChar) { Value = name };

            return await SelectAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }

        public async Task<int> DeleteByTypeAndNameAsync(SqlConnection connection, string type, string name = null)
        {
            string selectText = $"DELETE FROM {ObjectName}  WHERE [{nameof(GlobalParameterEntity.Type)}] = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND [{nameof(GlobalParameterEntity.Name)}] = @name";
            }

            var p = new SqlParameter("type", SqlDbType.NVarChar) { Value = type };

            if (String.IsNullOrEmpty(name))
            {
                return await ExecuteCommandNonQueryAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new SqlParameter("name", SqlDbType.NVarChar) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }
    }
}
