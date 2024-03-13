using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.SQLite
{
    public class ColumnInfo
    {
        public string Name;
        public DbType Type = DbType.String;
        public bool IsKey = false;
        public int Size = 256;
        public bool IsVirtual = false;
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
        public string ObjectName => $"{SchemaName}.{DbTableName}";

        // ReSharper disable once StaticMemberInGenericType
        public static List<ColumnInfo> DBColumnsStatic = new List<ColumnInfo>();

        //TODO REFACTOR - can do static, but need initialize all column in static constructor then 
        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        #region Command Insert/Update/Delete/Commit

        public async Task<int> InsertAsync(SqliteConnection connection, TEntity entity, SqliteTransaction transaction = null)
        {
            string commandText =
                $"INSERT INTO {ObjectName} ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => $"{c.Name}"))}) " +
                $"VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name))})";

            SqliteParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
        }

        public async Task<int> UpsertAsync(SqliteConnection connection, TEntity entity, SqliteTransaction transaction = null)
        {
            var commandText =
                $"INSERT INTO {ObjectName} ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => $"{c.Name}"))}) " +
                $"VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name))}) " +
                $"ON CONFLICT ({String.Join(",", DBColumns.Where(c => c.IsKey).Select(c => String.Format($"{c.Name}")))}) " + 
                $"DO UPDATE SET {String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => $"{c.Name} = @{c.Name}"))}";

            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
        }
        
        public virtual async Task<int> UpdateAsync(SqliteConnection connection, TEntity entity, SqliteTransaction transaction = null)
        {
            string command = $"UPDATE {ObjectName} " +
                             $"SET {String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => $"{c.Name} = @{c.Name}"))} " +
                             $"WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => $"{c.Name} = @{c.Name}"))}";

            SqliteParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllAsync(SqliteConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllWithPagingAsync(SqliteConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters,
            Paging paging)
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

                pagingText = $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            string selectText = $"SELECT * FROM {ObjectName}{orderText}{pagingText}";

            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }

        public async Task<TEntity> SelectByKeyAsync(SqliteConnection connection, object id)
        {
            var key = DBColumns.FirstOrDefault(c => c.IsKey);

            if (key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE {key.Name} = @p_id";
            var pId = new SqliteParameter("p_id", key.Type) {Value = ToDbValue(id, key.Type)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<TEntity[]> SelectByAsync<TSelect>(SqliteConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE {propertyName} = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new SqliteParameter(paramName, column.Type) {Value = ToDbValue(value, column.Type)};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<int> DeleteByAsync<TSelect>(SqliteConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, SqliteTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE {propertyName} = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new SqliteParameter(paramName, column.Type) {Value = ToDbValue(value, column.Type)};

            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByWithPagingAsync<TSelect, TSort>(SqliteConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value, Expression<Func<TEntity, TSort>> sortLambda,
            SortDirection sortDirection, Paging paging)
        {
            string selectorProperty = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string sortProperty = (sortLambda.Body as MemberExpression)?.Member.Name;

            string paramName = $"_{selectorProperty}";

            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            string selectText =
                String.Format(
                    $"SELECT * FROM {ObjectName} " + 
                    $"WHERE {selectorProperty} = @{paramName} " + 
                    $"ORDER BY {sortProperty} {sortDirection.UpperName()}{pagingText}");

            var column = DBColumns.Find(x => x.Name == selectorProperty);
            var param = new SqliteParameter(paramName, column.Type) {Value = ToDbValue(value, column.Type)};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public Task<int> DeleteAsync(SqliteConnection connection, object id, SqliteTransaction transaction = null)
        {
            var key = DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception($"Key for table {DbTableName} isn't defined.");

            var pId = new SqliteParameter("p_id", key.Type) {Value = ToDbValue(id, key.Type)};

            return ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE {key.Name} = @p_id", transaction, pId);
        }


        public async Task<int> GetCountAsync(SqliteConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*)  FROM {ObjectName}")
                .ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<int> GetCountByAsync<TSelect>(SqliteConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value, SqliteTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE {propertyName} = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new SqliteParameter(paramName, column.Type) {Value = ToDbValue(value, column.Type)};

            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public Task<int> ExecuteCommandNonQueryAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
        {
            return ExecuteCommandNonQueryAsync(connection, commandText, null, parameters);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(SqliteConnection connection, string commandText,
            SqliteTransaction transaction = null,
            params SqliteParameter[] parameters)
        {
            try
            {
                return await ExecuteCommandNonQueryInternalAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException(transaction == null);
            }
        }

        private async Task<int> ExecuteCommandNonQueryInternalAsync(SqliteConnection connection, string commandText,
            SqliteTransaction transaction = null,
            params SqliteParameter[] parameters)
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

        public async Task<object> ExecuteCommandScalarAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public async Task<object> ExecuteCommandScalarAsync(SqliteConnection connection, string commandText,
            SqliteTransaction transaction = null,
            params SqliteParameter[] parameters)
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
        
        private async Task<object> ExecuteCommandScalarInternalAsync(SqliteConnection connection, string commandText,
            SqliteTransaction transaction = null,
            params SqliteParameter[] parameters)
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

                command.CommandText = commandText;
                command.CommandTimeout = CommandTimeout;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var obj = await command.ExecuteScalarAsync().ConfigureAwait(false);
                return obj;
            }
        }

        public async Task<TEntity[]> SelectAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
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

        private async Task<TEntity[]> SelectInternalAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
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
                
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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
                                entity.SetValue(column.Name, FromDbValue(value, column.Type));
                            }
                        }

                        entities.Add(entity);
                    }
                }

                return entities.ToArray();
            }
        }

        public async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
        {
            try
            {
                return await SelectAsDictionaryInternalAsync(connection, commandText, parameters).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e.ToQueryException();
            }
        }

        private async Task<List<Dictionary<string, object>>> SelectAsDictionaryInternalAsync(SqliteConnection connection, string commandText,
            params SqliteParameter[] parameters)
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

        #endregion

        public async Task<int> InsertAllAsync(SqliteConnection connection, TEntity[] values, SqliteTransaction transaction = null)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            var parameters = new List<SqliteParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                var i1 = i;
                names.Add($"( {String.Join(",", DBColumns.Select(c => "@" + i1 + c.Name))} )");
                parameters.AddRange(DBColumns.Select(c => CreateParameter(values[i], c, i.ToString())).ToArray());
            }

            string command = $"INSERT INTO {ObjectName} ({String.Join(",", DBColumns.Select(c => $"{c.Name}"))}) " + 
                             $"VALUES {String.Join(",", names)}";


            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }

        public SqliteParameter CreateParameter(TEntity entity, ColumnInfo column, string namePrefix = "")
        {
            var value = ToDbValue(entity.GetValue(column.Name), column.Type);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new SqliteParameter(namePrefix + column.Name, column.Type) {Value = value, IsNullable = isNullable};
            return p;
        }
        
        protected static object ToDbValue<TValue>(TValue value, DbType type)
        {
            return type switch
            {
                DbType.Guid => value is Guid guid ? guid.ToString() : value,
                DbType.Boolean => value is bool b ? (object)(b ? 1 : 0) : value,
                DbType.Byte => value is byte b ? (object)(long)b : value,
                DbType.Int16 => value is int i ? (object)(long)i : value,
                DbType.DateTime2 => value is DateTime dateTime ? dateTime.Ticks : value,
                _ => value
            };
        }

        protected static object FromDbValue<TValue>(TValue dbValue, DbType type)
        {
            return type switch
            {
                DbType.Guid => dbValue is string s ? new Guid(s) : dbValue,
                DbType.Boolean => dbValue is long l ? (object)(l == 1) : dbValue,
                DbType.Byte => dbValue is long l ? (object)(byte)l : dbValue,
                DbType.Int16 => dbValue is long l ? (object)(int)l : dbValue,
                DbType.DateTime2 => dbValue is long l ? new DateTime(l) : dbValue,
                _ => dbValue
            };
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
                orderParameters.Select(x => $"{x.parameterName} {x.sortDirection.UpperName()}"));
            return result;
        }

        public string GetKeyOrFirstColumn()
        {
            ColumnInfo column = DBColumns.FirstOrDefault(c => c.IsKey) ?? DBColumns.First();
            return column.Name;
        }
    }
}
