using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.PostgreSQL.Models;


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
            string name = null, Sorting sort = null)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE \"{nameof(GlobalParameterEntity.Type)}\" = @type";

            var parameters = new List<NpgsqlParameter> {new("type", NpgsqlDbType.Varchar) {Value = type}};

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND \"{nameof(GlobalParameterEntity.Name)}\" = @name";
                parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = name});
            }

            if (sort != null)
            {
                selectText += $" ORDER BY \"{sort.FieldName}\" {sort.SortDirection.UpperName()}";
            }

            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        private QueryDefinition GetBasicSearchQuery(string type, string name = null)
        {
            var parameters = new List<NpgsqlParameter>();
            var selectText = $"FROM {ObjectName} WHERE \"{nameof(GlobalParameterEntity.Type)}\" = @type";
            
            parameters.Add(new NpgsqlParameter("type", NpgsqlDbType.Varchar) {Value = type});

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND \"{nameof(GlobalParameterEntity.Name)}\" LIKE @name";
                parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar) {Value = $"%{name}%"});
            }

            return new QueryDefinition() {Parameters = parameters, Query = selectText};
        }

        public async Task<GlobalParameterEntity[]> SearchByTypeAndNameWithPagingAsync(NpgsqlConnection connection, string type,
            string name = null, Paging paging = null, Sorting sort = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name);
            var parameters = queryDefinition.Parameters;

            sort ??= Sorting.Create(nameof(GlobalParameterEntity.Name));
            
            var selectText = $"SELECT * {queryDefinition.Query} ORDER BY \"{sort.FieldName}\" {sort.SortDirection.UpperName()}";

            if (paging != null)
            {
                selectText += " OFFSET @skip ROWS FETCH NEXT @size ROWS ONLY";
                
                parameters.Add(new NpgsqlParameter("skip", NpgsqlDbType.Integer) {Value = paging.SkipCount()});
                parameters.Add(new NpgsqlParameter("size", NpgsqlDbType.Integer) {Value = paging.PageSize});
            }

            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        public async Task<int> GetCountByTypeAndNameAsync(NpgsqlConnection connection, string type, string name = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name);
            var parameters = queryDefinition.Parameters;
            var selectText = $"SELECT COUNT(*) {queryDefinition.Query}";

            var result = await ExecuteCommandScalarAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
            var count = result == DBNull.Value ? 0 : result;
            return Convert.ToInt32(count);
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
