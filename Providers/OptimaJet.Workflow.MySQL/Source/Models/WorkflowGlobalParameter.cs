using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.MySQL.Models;

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

        public async Task<GlobalParameterEntity[]> SelectByTypeAndNameAsync(MySqlConnection connection, string type,
            string name = null, Sorting sort = null)
        {
            string selectText = $"SELECT * FROM {DbTableName}  WHERE `{nameof(GlobalParameterEntity.Type)}` = @type";

            var parameters = new List<MySqlParameter> {new ("type", MySqlDbType.VarString) {Value = type}};

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND `{nameof(GlobalParameterEntity.Name)}` = @name";
                parameters.Add(new MySqlParameter("name", MySqlDbType.VarString) { Value = name });
            }

            if (sort != null)
            {
                selectText += $" ORDER BY {sort.FieldName} {sort.SortDirection.UpperName()}";
            }

            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }
        
        private QueryDefinition GetBasicSearchQuery(string type, string name = null)
        {
            var parameters = new List<MySqlParameter>();
            var selectText = $"FROM {DbTableName} WHERE {nameof(GlobalParameterEntity.Type)} = @type";

            parameters.Add(new MySqlParameter("type", MySqlDbType.VarString) {Value = type});

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND {nameof(GlobalParameterEntity.Name)} LIKE @name";
                parameters.Add(new MySqlParameter("name", MySqlDbType.VarString) {Value = $"%{name}%"});
            }

            return new QueryDefinition() {Parameters = parameters, Query = selectText};
        }

        public async Task<GlobalParameterEntity[]> SearchByTypeAndNameWithPagingAsync(MySqlConnection connection, string type,
            string name = null, Paging paging = null, Sorting sort = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name);
            var parameters = queryDefinition.Parameters;

            sort ??= Sorting.Create(nameof(GlobalParameterEntity.Name));
            
            var selectText = $"SELECT * {queryDefinition.Query} ORDER BY {sort.FieldName} {sort.SortDirection.UpperName()}";

            if (paging != null)
            {
                selectText += " LIMIT @skip, @size";
                parameters.Add(new MySqlParameter("skip", MySqlDbType.Int32) {Value = paging.SkipCount()});
                parameters.Add(new MySqlParameter("size", MySqlDbType.Int32) {Value = paging.PageSize});
            }
            
            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        public async Task<int> GetCountByTypeAndNameAsync(MySqlConnection connection, string type, string name = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name);
            var parameters = queryDefinition.Parameters;
            var selectText = $"SELECT COUNT(*) {queryDefinition.Query}";
            
            var result = await ExecuteCommandScalarAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
            var count = result == DBNull.Value ? 0 : result;
            return Convert.ToInt32(count);
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
