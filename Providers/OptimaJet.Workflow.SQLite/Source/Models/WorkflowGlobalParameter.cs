using System.Data;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.SQLite.Models;


// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// Database object for workflow global parameter operations.
    /// </summary>
    public class WorkflowGlobalParameter : DbObject<GlobalParameterEntity>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkflowGlobalParameter"/>.
        /// </summary>
        /// <param name="schemaName">The database schema name.</param>
        /// <param name="commandTimeout">The command timeout in seconds.</param>
        public WorkflowGlobalParameter(string schemaName, int commandTimeout) : base(schemaName, "WorkflowGlobalParameter", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Type)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Name)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.Value)},
                new ColumnInfo {Name = nameof(GlobalParameterEntity.TenantId)}
            });
        }

        /// <summary>
        /// Selects global parameters by type with optional name filter, sorting, and tenant scope.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="name">The exact parameter name to filter by, or <c>null</c> for all names.</param>
        /// <param name="sort">The sorting settings, or <c>null</c> for default order.</param>
        /// <param name="tenantId">The tenant identifier, or <c>null</c> for shared parameters.</param>
        /// <returns>An array of matching global parameter entities.</returns>
        public async Task<GlobalParameterEntity[]> SelectByTypeAndNameAsync(SqliteConnection connection, string type,
            string name = null, Sorting sort = null, string tenantId = null)
        {
            string selectText = $"SELECT * FROM {ObjectName}  WHERE {nameof(GlobalParameterEntity.Type)} = @type";

            var parameters = new List<SqliteParameter> {new("type", DbType.String) {Value = type}};

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND {nameof(GlobalParameterEntity.Name)} = @name";
                parameters.Add(new SqliteParameter("name", DbType.String) {Value = name});
            }

            selectText = AddTenantCondition(selectText, parameters, tenantId);

            if (sort != null)
            {
                selectText += $" Order By {sort.FieldName} {sort.SortDirection.UpperName()}";
            }

            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        private QueryDefinition GetBasicSearchQuery(string type, string name = null, string tenantId = null)
        {
            var parameters = new List<SqliteParameter>();
            var selectText = $"FROM {ObjectName} WHERE {nameof(GlobalParameterEntity.Type)} = @type";

            parameters.Add(new SqliteParameter("type", DbType.String) {Value = type});

            if (!String.IsNullOrEmpty(name))
            {
                selectText += $" AND {nameof(GlobalParameterEntity.Name)} LIKE @name";
                parameters.Add(new SqliteParameter("name", DbType.String) {Value = $"%{name}%"});
            }

            selectText = AddTenantCondition(selectText, parameters, tenantId);

            return new QueryDefinition() {Parameters = parameters, Query = selectText};
        }

        /// <summary>
        /// Searches global parameters by type with a partial name match, paging, and tenant scope.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="name">A partial name to search for, or <c>null</c> for all names.</param>
        /// <param name="paging">The paging settings, or <c>null</c> to return all results.</param>
        /// <param name="sort">The sorting settings, or <c>null</c> for default order.</param>
        /// <param name="tenantId">The tenant identifier, or <c>null</c> for shared parameters.</param>
        /// <returns>An array of matching global parameter entities.</returns>
        public async Task<GlobalParameterEntity[]> SearchByTypeAndNameWithPagingAsync(SqliteConnection connection, string type,
            string name = null, Paging paging = null, Sorting sort = null, string tenantId = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name, tenantId);
            var parameters = queryDefinition.Parameters;
            
            sort ??= Sorting.Create(nameof(GlobalParameterEntity.Name));
            
            var selectText = $"SELECT * {queryDefinition.Query} ORDER BY {sort.FieldName} {sort.SortDirection.UpperName()}";

            if (paging != null)
            {
                selectText += " LIMIT @skip, @size";
                
                parameters.Add(new SqliteParameter("skip", DbType.Int32) {Value = paging.SkipCount()});
                parameters.Add(new SqliteParameter("size", DbType.Int32) {Value = paging.PageSize});
            }

            return await SelectAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the count of global parameters matching the specified type, optional name, and tenant scope.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="name">A partial name to filter by, or <c>null</c> for all names.</param>
        /// <param name="tenantId">The tenant identifier, or <c>null</c> for shared parameters.</param>
        /// <returns>The number of matching parameters.</returns>
        public async Task<int> GetCountByTypeAndNameAsync(SqliteConnection connection, string type, string name = null, string tenantId = null)
        {
            var queryDefinition = GetBasicSearchQuery(type, name, tenantId);
            var parameters = queryDefinition.Parameters;
            var selectText = $"SELECT COUNT(*) {queryDefinition.Query}";

            var result = await ExecuteCommandScalarAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
            var count = result == DBNull.Value ? 0 : result;
            return Convert.ToInt32(count);
        }

        /// <summary>
        /// Deletes global parameters matching the specified type, optional name, and tenant scope.
        /// </summary>
        /// <param name="connection">The SQLite connection.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="name">The exact parameter name to filter by, or <c>null</c> for all names.</param>
        /// <param name="tenantId">The tenant identifier, or <c>null</c> for shared parameters.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> DeleteByTypeAndNameAsync(SqliteConnection connection, string type, string name = null, string tenantId = null)
        {
            string selectText = $"DELETE FROM {ObjectName}  WHERE {nameof(GlobalParameterEntity.Type)} = @type";
            var parameters = new List<SqliteParameter> {new("type", DbType.String) {Value = type}};

            if (!String.IsNullOrEmpty(name))
            {
                selectText = selectText + $" AND {nameof(GlobalParameterEntity.Name)} = @name";
                parameters.Add(new SqliteParameter("name", DbType.String) {Value = name});
            }

            selectText = AddTenantCondition(selectText, parameters, tenantId);

            return await ExecuteCommandNonQueryAsync(connection, selectText, parameters.ToArray()).ConfigureAwait(false);
        }

        private static string AddTenantCondition(string selectText, List<SqliteParameter> parameters, string tenantId)
        {
            if (tenantId == null)
            {
                return selectText + $" AND {nameof(GlobalParameterEntity.TenantId)} IS NULL";
            }

            parameters.Add(new SqliteParameter("tenantId", DbType.String) {Value = tenantId});
            return selectText + $" AND {nameof(GlobalParameterEntity.TenantId)} = @tenantId";
        }
    }
}
