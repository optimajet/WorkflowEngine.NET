using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.MySQL
{
    public class ColumnInfo
    {
        public string Name;
        public MySqlDbType Type = MySqlDbType.VarString;
        public bool IsKey = false;
        public int Size = 256;
    }

    public class DbObject<TEntity> where TEntity : IEntity, new()
    {
        public DbObject(string dbTableName, int commandTimeout)
        {
            DbTableName = dbTableName;
            CommandTimeout = commandTimeout;
        }
        
        public int CommandTimeout { get; }
        public string DbTableName { get; }

        // ReSharper disable once StaticMemberInGenericType
        public static List<ColumnInfo> DBColumnsStatic = new List<ColumnInfo>();

        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        #region Command Insert/Update/Delete/Commit

        public virtual async Task<int> InsertAsync(MySqlConnection connection, TEntity entity, MySqlTransaction transaction = null)
        {
            string command = $"INSERT INTO {DbTableName} ({String.Join(",", DBColumns.Select(c => $"`{c.Name}`"))}) " + 
                             $"VALUES ({String.Join(",", DBColumns.Select(c => "@" + c.Name))})";

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();
            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }
        
        public async Task<int> UpsertAsync(MySqlConnection connection, TEntity entity, MySqlTransaction transaction = null)
        {
            var commandText =
                $"INSERT INTO {DbTableName} ({String.Join(",", DBColumns.Select(c => $"`{c.Name}`"))}) " +
                $"VALUES ({String.Join(",", DBColumns.Select(c => "@" + c.Name))}) " +
                $"ON DUPLICATE KEY UPDATE" +
                $"{String.Join(",", DBColumns.Select(c => $"`{c.Name}` = VALUES(`{c.Name}`)"))}";

            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters)
                .ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync(MySqlConnection connection, TEntity entity, MySqlTransaction transaction = null)
        {
            string command = $@"UPDATE {DbTableName} " + 
                             $"SET {String.Join(",", DBColumns.Where(c => !c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name)))} " + 
                             $"WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name)))}";

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }

        

        public async Task<TEntity[]> SelectAllAsync(MySqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {DbTableName}").ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllWithPagingAsync(MySqlConnection connection,
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

                pagingText = $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            string selectText = String.Format($"SELECT * FROM {DbTableName}{orderText}{pagingText}");

            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByAsync<TSelect>(MySqlConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new MySqlParameter(paramName, column.Type) { Value = ToDbValue(value, column.Type) };

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByWithPagingAsync<TSelect, TSort>(MySqlConnection connection,
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

            string selectText = String.Format($"SELECT * FROM {DbTableName} " + 
                                              $"WHERE `{selectorProperty}` = @{paramName} " + 
                                              $"ORDER BY `{sortProperty}` {sortDirection.UpperName()}{pagingText}");

            var column = DBColumns.Find(x => x.Name == selectorProperty);
            var param = new MySqlParameter(paramName, column.Type) { Value = ToDbValue(value, column.Type) };

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<int> DeleteByAsync<TSelect>(MySqlConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, MySqlTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new MySqlParameter(paramName, column.Type) { Value = ToDbValue(value, column.Type) };

            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }

        public async Task<TEntity> SelectByKeyAsync(MySqlConnection connection, object id)
        {
            ColumnInfo key = DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {DbTableName} WHERE `{key.Name}` = @p_id";
            
            var pId = new MySqlParameter("p_id", key.Type) {Value = ToDbValue(id, key.Type)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<int> DeleteAsync(MySqlConnection connection, object id, MySqlTransaction transaction = null)
        {
            ColumnInfo key = DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            var pId = new MySqlParameter("p_id", key.Type) {Value = ToDbValue(id, key.Type)};

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `{key.Name}` = @p_id", transaction, pId).ConfigureAwait(false);
        }

        public async Task<int> GetCountAsync(MySqlConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*)  FROM {DbTableName}")
                .ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<int> GetCountByAsync<TSelect>(MySqlConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value, MySqlTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new MySqlParameter(paramName, column.Type) { Value = ToDbValue(value, column.Type) };
            
            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public Task<int> ExecuteCommandNonQueryAsync(MySqlConnection connection, string commandText,
            params MySqlParameter[] parameters)
        {
            return ExecuteCommandNonQueryAsync(connection, commandText, null, parameters);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(MySqlConnection connection, string commandText,
            MySqlTransaction transaction = null, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            int cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return cnt;
        }

        public async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, string commandText,
            params MySqlParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, string commandText,
            MySqlTransaction transaction = null, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
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

        public async Task<TEntity[]> SelectAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            
            var entities = new List<TEntity>();
            
            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var entity = new TEntity();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    ColumnInfo column = DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (column != null)
                    {
                        var value = await reader.IsDBNullAsync(i).ConfigureAwait(false) ? null : reader.GetValue(i);
                        entity.SetValue(column.Name, FromDbValue(value, column.Type));
                    }
                }

                entities.Add(entity);
            }

            return entities.ToArray();
        }

        public async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(MySqlConnection connection, string commandText,
            params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
            
            command.Connection = connection;
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            var res = new List<Dictionary<string, object>>();
            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
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

            return res;
        }

        #endregion


        public async Task<int> InsertAllAsync(MySqlConnection connection, TEntity[] values, MySqlTransaction transaction = null)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            var parameters = new List<MySqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                int i1 = i;
                names.Add($"({String.Join(",", DBColumns.Select(c => "@" + i1 + c.Name))})");
                parameters.AddRange(DBColumns.Select(c => CreateParameter(values[i], c, i.ToString())).ToArray());
            }

            string command = $"INSERT INTO {DbTableName} ({String.Join(",", DBColumns.Select(c => $"`{c.Name}`"))}) " + 
                             $"VALUES {String.Join(",", names)}";

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }

        public virtual MySqlParameter CreateParameter(TEntity entity, ColumnInfo c, string namePrefix = "")
        {
            var p = new MySqlParameter(namePrefix + c.Name, c.Type) {Value = ToDbValue(entity.GetValue(c.Name), c.Type)};
            return p;
        }

        protected static object ToDbValue<TValue>(TValue value, MySqlDbType type)
        {
            return type switch
            {
                MySqlDbType.Binary => value is Guid guid ? (object)guid.ToByteArray() : value,
                MySqlDbType.Byte => value is byte b ? (object)(sbyte)b : value,
                MySqlDbType.Bit => value is bool b ? (object)(ulong)(b ? 1 : 0) : value,
                _ => value
            };
        }

        protected static object FromDbValue<TValue>(TValue dbValue, MySqlDbType type)
        {
            return type switch
            {
                MySqlDbType.Binary => dbValue is byte[] bytes ? (object)new Guid(bytes) : dbValue,
                MySqlDbType.Byte => dbValue is sbyte sb ? (object)(byte)sb : dbValue,
                MySqlDbType.Bit => dbValue is ulong ul ? (object)(ul == 1) : dbValue,
                _ => dbValue
            };
        }

        private static string GetOrderParameters(List<(string parameterName, SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ", orderParameters.Select(x => $"`{x.parameterName}` {x.sortDirection.UpperName()}"));
            return result;
        }

        public string GetKeyOrFirstColumn()
        {
            ColumnInfo column = DBColumns.FirstOrDefault(c => c.IsKey) ?? DBColumns.First();
            return column.Name;
        }
    }
}
