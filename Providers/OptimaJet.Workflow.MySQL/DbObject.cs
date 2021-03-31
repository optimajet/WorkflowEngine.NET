using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
    public class DbObject<T> where T : DbObject<T>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        public static string DbTableName;

        
        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }
      
        #region Command Insert/Update/Delete/Commit
        public virtual async Task<int> InsertAsync(MySqlConnection connection, MySqlTransaction transaction = null)
        {
            string command = $"INSERT INTO {DbTableName} ({String.Join(",", DBColumns.Select(c => $"`{c.Name}`"))}) VALUES ({String.Join(",", DBColumns.Select(c => "@" + c.Name))})";

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(c)).ToArray();
            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync(MySqlConnection connection, MySqlTransaction transaction = null)
        {
            string command = String.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    DbTableName,
                    String.Join(",", DBColumns.Where(c => !c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name))),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name))));

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
            
        }

        protected static object ValueForDb<TValue>( TValue value)
        {
            object valueForDb = value;
            if (value is Guid test)
            {
                valueForDb =  test.ToByteArray();
            }

            return valueForDb;
        }
        public static async Task<T[]> SelectAllAsync(MySqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {DbTableName}").ConfigureAwait(false);
        }
        
        public static async Task<T[]>  SelectAllWithPagingAsync(MySqlConnection connection, List<(string parameterName,SortDirection sortDirection)> orderParameters, Paging paging)
        {
            orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();
            
            string pagingText = String.Empty;
            string orderText = String.Empty;
            
            if (paging != null)
            {
                //default sort for paging
                if (orderParameters.Count < 1)
                {
                    orderParameters.Add((GetKeyOrFirstColumn(),SortDirection.Asc));
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
        
        public static async Task<T[]> SelectByAsync<TSelect>(MySqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");
            var param = new MySqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = ValueForDb(value) };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<T[]> SelectByWithPagingAsync<TSelect, TSort>(MySqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, Expression<Func<T, TSort>> sortLambda, SortDirection sortDirection, Paging paging)
        {
            var t = new T();
            string selectorProperty = (selectorLambda.Body as MemberExpression).Member.Name;
            string sortProperty =(sortLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{selectorProperty}";
            
            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }
            
            string selectText = String.Format($"SELECT * FROM {DbTableName} WHERE `{selectorProperty}` = @{paramName} ORDER BY `{sortProperty}` {sortDirection.UpperName()}{pagingText}");
            
            var param = new MySqlParameter(paramName, t.DBColumns.Find(x=>x.Name == selectorProperty).Type) { Value = ValueForDb(value) };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public static async Task<int> DeleteByAsync<TSelect>(MySqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, MySqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");
            
            var param = new MySqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = ValueForDb(value) };
            
            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }
        
        public static async Task<T> SelectByKeyAsync(MySqlConnection connection, object id)
        {
            var t = new T();

            ColumnInfo key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {DbTableName} WHERE `{key.Name}` = @p_id";
            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<int> DeleteAsync(MySqlConnection connection, object id, MySqlTransaction transaction = null)
        {
            var t = new T();
            ColumnInfo key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return await ExecuteCommandNonQueryAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `{key.Name}` = @p_id", transaction, pId).ConfigureAwait(false);
        }
        
        public static async Task<int> GetCountAsync(MySqlConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*)  FROM {DbTableName}")
                .ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static async Task<int> GetCountByAsync<TSelect>(MySqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, MySqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {DbTableName} WHERE `{propertyName}` = @{paramName}");
            
            var param = new MySqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = ValueForDb(value) };
            var result  = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static Task<int> ExecuteCommandNonQueryAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            return ExecuteCommandNonQueryAsync(connection, commandText, null, parameters);
        }

        public static async Task<int> ExecuteCommandNonQueryAsync(MySqlConnection connection, string commandText, MySqlTransaction transaction = null, params MySqlParameter[] parameters)
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
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            int cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return cnt;
        }

        public static async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }
        
        public static async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, string commandText, MySqlTransaction transaction = null, params MySqlParameter[] parameters)
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
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            return await command.ExecuteScalarAsync().ConfigureAwait(false);
        }
        

        public static async Task<T[]> SelectAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            var res = new List<T>();
            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var item = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    ColumnInfo column = item.DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (column != null)
                    {
                        item.SetValue(column.Name, await reader.IsDBNullAsync(i).ConfigureAwait(false) ? null : reader.GetValue(i));
                    }
                }
                res.Add(item);
            }

            return res.ToArray();
        }
        
        public static async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using MySqlCommand command = connection.CreateCommand();
            command.Connection = connection;
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


        public static async Task<int> InsertAllAsync(MySqlConnection connection, T[] values, MySqlTransaction transaction = null)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            var parameters = new List<MySqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                names.Add($"({String.Join(",", values[i].DBColumns.Select(c => "@" + i + c.Name))})");
                parameters.AddRange(values[i].DBColumns.Select(c => values[i].CreateParameter(c, i.ToString())).ToArray());
            }
            
            string command = $"INSERT INTO {DbTableName} ({String.Join(",", values.First().DBColumns.Select(c => $"`{c.Name}`"))}) VALUES {String.Join(",", names)}";
            
            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }
        
        public virtual MySqlParameter CreateParameter(ColumnInfo c, string namePrefix = "")
        {
            var p = new MySqlParameter(namePrefix+c.Name, c.Type) {Value = GetValue(c.Name)};
            return p;
        }
        
        private static object ConvertToDbCompatibilityType(object obj)
        {
            if (obj is Guid guid)
            {
                return guid.ToByteArray();
            }

            return obj;
        }
        
        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"`{x.parameterName}` {x.sortDirection.UpperName()}"));
            return result;
        }
        
        public static string GetKeyOrFirstColumn()
        {
            var t = new T();
            ColumnInfo column = t.DBColumns.FirstOrDefault(c => c.IsKey) ?? t.DBColumns.First();
            return column.Name;
        }
    }
}
