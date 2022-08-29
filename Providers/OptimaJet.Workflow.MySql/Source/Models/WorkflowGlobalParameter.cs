using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OptimaJet.Workflow.Core.Entities;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowGlobalParameter : DbObject<GlobalParameterEntity>
    {
        public WorkflowGlobalParameter(int commandTimeout) : base("workflowglobalparameter", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Type)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Name)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Value)}
            });
        }

        public async Task<GlobalParameterEntity[]> SelectByTypeAndNameAsync(MySqlConnection connection, string type, string name = null)
        {
            string selectText = $"SELECT * FROM {DbTableName}  WHERE `{nameof(GlobalParameterEntity.Type)}` = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND `{nameof(GlobalParameterEntity.Name)}` = @name";
            }

            var p = new MySqlParameter("type", MySqlDbType.VarString) {Value = type};

            if (String.IsNullOrEmpty(name))
            {
                return await SelectAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new MySqlParameter("name", MySqlDbType.VarString) { Value = name };

            return await SelectAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }

        public async Task<int> DeleteByTypeAndNameAsync(MySqlConnection connection, string type, string name = null)
        {
            string selectText = $"DELETE FROM {DbTableName}  WHERE `{nameof(GlobalParameterEntity.Type)}` = @type";

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND `{nameof(GlobalParameterEntity.Name)}` = @name";
            }

            var p = new MySqlParameter("type", MySqlDbType.VarString) { Value = type };

            if (String.IsNullOrEmpty(name))
            {
                return await ExecuteCommandNonQueryAsync(connection, selectText, p).ConfigureAwait(false);
            }

            var p1 = new MySqlParameter("name", MySqlDbType.VarString) { Value = name };

            return await ExecuteCommandNonQueryAsync(connection, selectText, p, p1).ConfigureAwait(false);
        }
    }
}
