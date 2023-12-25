using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.DbPersistence
{
    public class ColumnInfo
    {
        public string Name;
        public SqlDbType Type = SqlDbType.NVarChar;
        public bool IsKey = false;
        public int Size = 256;
        public bool IsVirtual = false;
    }

    public static class DBColumnsExtensions
    {
        public static void AddColumn(this Dictionary<string, ColumnInfo> dbColumns, ColumnInfo column)
        {
            dbColumns.Add(column.Name, column);
        }

        public static void AddColumns(this Dictionary<string, ColumnInfo> dbColumns, List<ColumnInfo> columns)
        {
            foreach (var column in columns)
            {
                dbColumns.Add(column.Name, column);
            }
        }

        public static void AddColumns(this Dictionary<string, ColumnInfo> dbColumns, ColumnInfo[] columns)
        {
            foreach (var column in columns)
            {
                dbColumns.Add(column.Name, column);
            }
        }
    }

    public class DbObject<TEntity> where TEntity : IEntity, new()
    {
        public DbObject(string schemaName, string dbTableName, int commandTimeout)
        {
            SchemaName = schemaName;
            DbTableName = dbTableName;
            CommandTimeout = commandTimeout;
        }

        public int CommandTimeout { get; }
        public string SchemaName { get; }
        public string DbTableName { get; }
        public string ObjectName =>
            !String.IsNullOrWhiteSpace(SchemaName)
                ? $"[{SchemaName}].[{DbTableName}]"
                : $"[{DbTableName}]";

        // ReSharper disable once StaticMemberInGenericType
        public static List<ColumnInfo> DBColumnsStatic = new List<ColumnInfo>();

        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        #region Command Insert/Update/Delete/Commit

        public async Task<int> InsertAsync(SqlConnection connection, TEntity entity, SqlTransaction transaction = null)
        {
            var commandText =
                $"INSERT INTO {ObjectName} (" +
                String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => $"[{c.Name}]")) +
                $") VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name))})";

            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters)
                .ConfigureAwait(false);
        }

        public async Task<int> UpsertAsync(SqlConnection connection, TEntity entity, SqlTransaction transaction = null)
        {
            if (transaction != null)
            {
                return await UpsertInternalAsync(connection, entity, transaction).ConfigureAwait(false);
            }

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
                
            using var tnxInternal = connection.BeginTransaction();
                
            int rowcount = await UpsertInternalAsync(connection, entity, tnxInternal).ConfigureAwait(false);
                
            tnxInternal.Commit();
                
            return rowcount;
        }

        private async Task<int> UpsertInternalAsync(SqlConnection connection, TEntity entity, SqlTransaction transaction = null)
        {
            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();
            
            string command =
                $@"UPDATE {ObjectName} WITH (UPDLOCK, SERIALIZABLE) SET " +
                String.Join(",",DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name))) +
                $" WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name)))}";

            int rowcount = await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);

            if (rowcount == 0)
            {
                rowcount = await InsertAsync(connection, entity, transaction).ConfigureAwait(false);
            }

            return rowcount;
        } 

        public async Task<int> UpdateAsync(SqlConnection connection, TEntity entity, SqlTransaction transaction = null)
        {
            string command =
                $@"UPDATE {ObjectName} SET " +
                String.Join(",",
                    DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name))) +
                $" WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name)))}";

            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters)
                .ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllAsync(SqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllWithPagingAsync(SqlConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters, Paging paging)
        {
            orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();

            string pagingText = String.Empty;
            string orderText = String.Empty;

            if (paging != null)
            {
                //default sort for paging
                if (orderParameters.Count < 1)
                {
                    orderParameters.Add((GetKeyOrFirstColumn(), SortDirection.Asc));
                }

                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            string selectText = $"SELECT * FROM {ObjectName}{orderText}{pagingText}";

            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByAsync<TSelect>(SqlConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");

            var param = new SqlParameter(paramName, DBColumns.Find(x => x.Name == propertyName).Type) {Value = value};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<int> DeleteByAsync<TSelect>(SqlConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, SqlTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");

            var param = new SqlParameter(paramName, DBColumns.Find(x => x.Name == propertyName).Type) {Value = value};

            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByWithPagingAsync<TSelect, TSort>(SqlConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value, Expression<Func<TEntity, TSort>> sortLambda,
            SortDirection sortDirection,
            Paging paging)
        {
            string selectorProperty = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string sortProperty = (sortLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{selectorProperty}";
            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE [{selectorProperty}] = @{paramName}" +
                                              $" ORDER BY [{sortProperty}] {sortDirection.UpperName()}{pagingText}");

            var param = new SqlParameter(paramName, DBColumns.Find(x => x.Name == selectorProperty).Type) {Value = value};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }


        public async Task<TEntity> SelectByKeyAsync(SqlConnection connection, object id)
        {
            var key = DBColumns.FirstOrDefault(c => c.IsKey);

            if (key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE [{key.Name}] = @p_id";
            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<int> GetCountAsync(SqlConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*) FROM {ObjectName}")
                .ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<int> GetCountByAsync<TSelect>(SqlConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, SqlTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");

            var param = new SqlParameter(paramName, DBColumns.Find(x => x.Name == propertyName).Type) {Value = value};
            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }


        public async Task<int> DeleteAsync(SqlConnection connection, object id, SqlTransaction transaction = null)
        {
            var key = DBColumns.FirstOrDefault(c => c.IsKey);

            if (key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE [{key.Name}] = @p_id", transaction, pId)
                .ConfigureAwait(false);
        }

        private async Task<int> ExecuteCommandInternalAsync(SqlConnection connection, string commandText, SqlTransaction transaction,
            SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                command.CommandTimeout = CommandTimeout;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return cnt;
            }
        }

        public async Task<TEntity[]> SelectAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
        {
            try
            {
                return await SelectInternalAsync(connection, commandText, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException();
            }
        }

        public async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(SqlConnection connection, string commandText,
            params SqlParameter[] parameters)
        {
            try
            {
                return await SelectInternalAsDictionaryAsync(connection, commandText, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException();
            }
        }

        public async Task<int> ExecuteCommandNonQueryAsync(SqlConnection connection, string commandText,
            params SqlParameter[] parameters)
        {
            return await ExecuteCommandNonQueryAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(SqlConnection connection, string commandText,
            SqlTransaction transaction = null, params SqlParameter[] parameters)
        {
            try
            {
                return await ExecuteCommandInternalAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException(transaction == null);
            }
        }

        public async Task<object> ExecuteCommandScalarAsync(SqlConnection connection, string commandText,
            params SqlParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public async Task<object> ExecuteCommandScalarAsync(SqlConnection connection, string commandText,
            SqlTransaction transaction = null, params SqlParameter[] parameters)
        {
            try
            {
                return await ExecuteCommandScalarInternalAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException(transaction == null);
            }
        }

        private async Task<object> ExecuteCommandScalarInternalAsync(SqlConnection connection, string commandText,
            SqlTransaction transaction, params SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                command.CommandTimeout = CommandTimeout;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                return await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }

        private async Task<TEntity[]> SelectInternalAsync(SqlConnection connection, string commandText, SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandTimeout = CommandTimeout;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);

                var entities = new List<TEntity>();

                using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var entity = new TEntity();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var column = DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                            if (column != null)
                            {
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                entity.SetValue(column.Name, value);
                            }
                        }

                        entities.Add(entity);
                    }
                }

                return entities.ToArray();
            }
        }

        private async Task<List<Dictionary<string, object>>> SelectInternalAsDictionaryAsync(SqlConnection connection,
            string commandText, SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandTimeout = CommandTimeout;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var res = new List<Dictionary<string, object>>();

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            item.Add(name, reader.IsDBNull(i) ? null : reader.GetValue(i));
                        }

                        res.Add(item);
                    }
                }

                return res;
            }
        }

        #endregion Command Insert/Update/Delete/Commit

        public async Task<int> InsertAllAsync(SqlConnection connection, TEntity[] entities,
            SqlTransaction transaction = null)
        {
            if (entities.Length < 1)
            {
                return 0;
            }
            
            if (entities.Length * DBColumns.Count < 2100)
            {
                return await InsertAllInternalAsync(connection, entities, transaction).ConfigureAwait(false);
            }

            bool needCommit = false;
            
            if (transaction == null)
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }
                transaction = connection.BeginTransaction();
                needCommit = true;
            }

            int step = 2100 / DBColumns.Count - 1;
            int result = 0;
            for (int i = 0; i < entities.Length; i += step)
            {
                TEntity[] entitiesSlice = entities.Skip(i).Take(step).ToArray();
                result += await InsertAllInternalAsync(connection, entitiesSlice, transaction).ConfigureAwait(false);
            }

            if (needCommit)
            {
                transaction.Commit();
            }

            return result;
        }

        private async Task<int> InsertAllInternalAsync(SqlConnection connection, TEntity[] entities,
            SqlTransaction transaction = null)
        {
            var parameters = new List<SqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < entities.Length; i++)
            {
                var i1 = i;
                names.Add("(" + String.Join(",", DBColumns.Select(c => "@" + i1 + c.Name)) + ")");
                parameters.AddRange(DBColumns.Select(c => CreateParameter(entities[i], c, i.ToString())).ToArray());
            }

            string command =
                $"INSERT INTO {ObjectName} (" +
                String.Join(",", DBColumns.Select(c => $"[{c.Name}]")) +
                $") VALUES {String.Join(",", names)}";

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray())
                .ConfigureAwait(false);
        }

        public static object ConvertToDbCompatibilityType(object obj)
        {
            return obj;
        }

        public virtual SqlParameter CreateParameter(TEntity entity, ColumnInfo column, string namePrefix = "")
        {
            var value = entity.GetValue(column.Name);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new SqlParameter(namePrefix + column.Name, column.Type) {Value = value, IsNullable = isNullable};
            return p;
        }

        private static bool IsNullable<T1>(T1 obj)
        {
            if (obj == null) return true; //-V3111
            Type type = typeof(T1);
            if (!type.GetTypeInfo().IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true;
            return false;
        }

        private static string GetOrderParameters(List<(string parameterName, SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"[{x.parameterName}] {x.sortDirection.UpperName()}"));
            return result;
        }

        public string GetKeyOrFirstColumn()
        {
            return DBColumns.FirstOrDefault(c => c.IsKey)?.Name ?? DBColumns.First().Name;
        }
    }
}
