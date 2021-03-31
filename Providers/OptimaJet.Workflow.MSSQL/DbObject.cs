using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using OptimaJet.Workflow.Core;
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

    public abstract class DbObject
    {
        public static string SchemaName { get; set; }
    }

    public class DbObject<T> : DbObject where T : DbObject<T>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        public static string DbTableName;

        public static string ObjectName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(SchemaName))
                    return $"[{SchemaName}].[{DbTableName}]";
                return $"[{DbTableName}]";
            }
        }

        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }
      
        #region Command Insert/Update/Delete/Commit
        
        public async Task<int> InsertAsync(SqlConnection connection, SqlTransaction transaction = null)
        {
            var commandText =
                $"INSERT INTO {ObjectName} ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => $"[{c.Name}]"))}) VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name))})";
            var parameters = DBColumns.Select(CreateParameter).ToArray();
            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters)
                .ConfigureAwait(false);
        }
        
        
        public async Task<int> UpdateAsync(SqlConnection connection, SqlTransaction transaction = null)
        {
            string command =
                $@"UPDATE {ObjectName} SET {String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name)))} WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("[{0}] = @{0}", c.Name)))}";

            var parameters = DBColumns.Select(CreateParameter).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters)
                .ConfigureAwait(false);
        }

        public static async Task<T[]> SelectAllAsync(SqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }
        public static async Task<T[]> SelectAllWithPagingAsync(SqlConnection connection, List<(string parameterName,SortDirection sortDirection)> orderParameters, Paging paging)
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
                
                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }
            
            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }
            
            string selectText = $"SELECT * FROM {ObjectName}{orderText}{pagingText}";

            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
        
        public static async Task<T[]> SelectByAsync<TSelect>(SqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");
            
            var param = new SqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<int> DeleteByAsync<TSelect>(SqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, SqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");
            
            var param = new SqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };
            
            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }

        public static async Task<T[]> SelectByWithPagingAsync<TSelect, TSort>(SqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, Expression<Func<T, TSort>> sortLambda, SortDirection sortDirection, Paging paging)
        {
            var t = new T();
            string selectorProperty = (selectorLambda.Body as MemberExpression).Member.Name;
            string sortProperty =(sortLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{selectorProperty}";
            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }
            
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE [{selectorProperty}] = @{paramName} ORDER BY [{sortProperty}] {sortDirection.UpperName()}{pagingText}");
            
            var param = new SqlParameter(paramName, t.DBColumns.Find(x=>x.Name == selectorProperty).Type) { Value = value };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        
        public static async Task<T> SelectByKeyAsync(SqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE [{key.Name}] = @p_id";
            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<int> GetCountAsync(SqlConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*) FROM {ObjectName}")
                .ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static async Task<int> GetCountByAsync<TSelect>(SqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, SqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE [{propertyName}] = @{paramName}");
            
            var param = new SqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };
            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);

        }


        public static async Task<int> DeleteAsync(SqlConnection connection, object id, SqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception($"Key for table {ObjectName} isn't defined.");

            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE [{key.Name}] = @p_id", transaction, pId)
                .ConfigureAwait(false);
        }

        private static async Task<int> ExecuteCommandInternalAsync(SqlConnection connection, string commandText, SqlTransaction transaction, SqlParameter[] parameters)
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
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return cnt;
            }
        }

        public static async Task<T[]> SelectAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
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
        
        public static async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
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
        
        public static async Task<int> ExecuteCommandNonQueryAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
        {
            return await ExecuteCommandNonQueryAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public static async Task<int> ExecuteCommandNonQueryAsync(SqlConnection connection, string commandText, SqlTransaction transaction = null,  params SqlParameter[] parameters)
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

        public static async Task<object> ExecuteCommandScalarAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }

        public static async Task<object> ExecuteCommandScalarAsync(SqlConnection connection, string commandText, SqlTransaction transaction = null, params SqlParameter[] parameters)
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

        private static async Task<object> ExecuteCommandScalarInternalAsync(SqlConnection connection, string commandText, SqlTransaction transaction, params SqlParameter[] parameters)
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
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                return  await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }
        
        private static async Task<T[]> SelectInternalAsync(SqlConnection connection, string commandText, SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var res = new List<T>();
                
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        T item = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var column = item.DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                            if (column != null)
                            {
                                item.SetValue(column.Name, reader.IsDBNull(i) ? null : reader.GetValue(i));
                            }
                        }

                        res.Add(item);
                    }
                }

                return res.ToArray();
            }
        }
        
        private static async Task<List<Dictionary<string, object>>> SelectInternalAsDictionaryAsync(SqlConnection connection, string commandText, SqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
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

        public static async Task<int> InsertAllAsync(SqlConnection connection, T[] values, SqlTransaction transaction = null)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            var parameters = new List<SqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                names.Add("("+String.Join(",", values[i].DBColumns.Select(c => "@" + i + c.Name))+")");
                parameters.AddRange(values[i].DBColumns.Select(c => values[i].CreateParameter(c, i.ToString())).ToArray());
            }

            string command =
                $"INSERT INTO {ObjectName} ({String.Join(",", values.First().DBColumns.Select(c => $"[{c.Name}]"))}) VALUES {String.Join(",", names)}";


            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }

        public static object ConvertToDbCompatibilityType(object obj)
        {
            return obj;
        }

        public virtual SqlParameter CreateParameter(ColumnInfo c)
        {
            var value = GetValue(c.Name);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new SqlParameter(c.Name, c.Type) {Value = value, IsNullable = isNullable};
            return p;
        }
        
        public virtual SqlParameter CreateParameter(ColumnInfo c, string namePrefix)
        {
            var value = GetValue(c.Name);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new SqlParameter(namePrefix + c.Name, c.Type) {Value = value, IsNullable = isNullable};
            return p;
        }
        
        private static bool IsNullable<T1>(T1 obj)
        {
            if (obj == null) return true;  //-V3111
            Type type = typeof (T1);
            if (!type.GetTypeInfo().IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true; 
            return false;
        }

        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"[{x.parameterName}] {x.sortDirection.UpperName()}"));
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
