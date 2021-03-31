using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.PostgreSQL
{
    public class ColumnInfo
    {
        public string Name;
        public NpgsqlDbType Type = NpgsqlDbType.Varchar;
        public bool IsKey = false;
        public int Size = 256;
        public bool IsVirtual = false;
    }

    public abstract class DbObject
    {
        public static string SchemaName { get; set; }
    }

    public class DbObject<T> : DbObject where T : DbObject<T>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        public static string DbTableName;

        public static string ObjectName => $"\"{SchemaName}\".\"{DbTableName}\"";

        //TODO REFACTOR - can do static, but need initialize all column in static constructor then 
        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        public IEnumerable<string> DBColumnNames => DBColumns.Select(column => column.Name);
        

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }
      
        #region Command Insert/Update/Delete/Commit
        
        public async Task<int> InsertAsync(NpgsqlConnection connection, NpgsqlTransaction transaction = null)
        {
            string commandText = String.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                ObjectName,
                String.Join(",", DBColumns.Where(c=> !c.IsVirtual).Select(c => $"\"{c.Name}\"")),
                String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name)));
            NpgsqlParameter[] parameters = DBColumns.Select(CreateParameter).ToArray();
            return await ExecuteCommandNonQueryAsync(connection, commandText, transaction, parameters).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction transaction = null)
        {
            string command =
                $"UPDATE {ObjectName} SET {String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => $"\"{c.Name}\" = @{c.Name}"))} WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => $"\"{c.Name}\" = @{c.Name}"))}";

            NpgsqlParameter[] parameters = DBColumns.Select(CreateParameter).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
          
        }

        public static async Task<T[]> SelectAllAsync(NpgsqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }

        public static async Task<T[]> SelectAllWithPagingAsync(NpgsqlConnection connection, List<(string parameterName,SortDirection sortDirection)> orderParameters, Paging paging)
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
            
            string selectText = $"SELECT * FROM {ObjectName}{orderText}{pagingText}";
            
            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }
        
        public static async Task<T> SelectByKeyAsync(NpgsqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception($"Key for table {DbTableName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE \"{key.Name}\" = @p_id";
            var pId = new NpgsqlParameter("p_id", key.Type) {Value = id};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }
        
        public static async Task<T[]> SelectByAsync<TSelect>(NpgsqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE \"{propertyName}\" = @{paramName}");
            
            var param = new NpgsqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<int> DeleteByAsync<TSelect>(NpgsqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, NpgsqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE \"{propertyName}\" = @{paramName}");
            
            var param = new NpgsqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };
            
            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }
        
        public static async Task<T[]> SelectByWithPagingAsync<TSelect, TSort>(NpgsqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, Expression<Func<T, TSort>> sortLambda, SortDirection sortDirection, Paging paging)
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
            
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE \"{selectorProperty}\" = @{paramName} ORDER BY \"{sortProperty}\" {sortDirection.UpperName()}{pagingText}");
            
            var param = new NpgsqlParameter(paramName, t.DBColumns.Find(x=>x.Name == selectorProperty).Type) { Value = value };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static Task<int> DeleteAsync(NpgsqlConnection connection, object id, NpgsqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception($"Key for table {DbTableName} isn't defined.");

            var pId = new NpgsqlParameter("p_id", key.Type) {Value = id};

            return ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE \"{key.Name}\" = @p_id", transaction, pId);
        }

        
        public static async Task<int> GetCountAsync(NpgsqlConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT Count(*)  FROM {ObjectName}")
                .ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static async Task<int> GetCountByAsync<TSelect>(NpgsqlConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, NpgsqlTransaction transaction = null)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE \"{propertyName}\" = @{paramName}");
            
            var param = new NpgsqlParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type) { Value = value };

            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static Task<int> ExecuteCommandNonQueryAsync(NpgsqlConnection connection, string commandText,
            params NpgsqlParameter[] parameters)
        {
            return ExecuteCommandNonQueryAsync(connection, commandText, null, parameters);
        }

        public static async Task<int> ExecuteCommandNonQueryAsync(NpgsqlConnection connection, string commandText, NpgsqlTransaction transaction = null,
            params NpgsqlParameter[] parameters)
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
        public static async Task<object> ExecuteCommandScalarAsync(NpgsqlConnection connection, string commandText,
            params NpgsqlParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }
        
        public static async Task<object> ExecuteCommandScalarAsync(NpgsqlConnection connection, string commandText, NpgsqlTransaction transaction = null,
            params NpgsqlParameter[] parameters)
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
                var obj = await command.ExecuteScalarAsync().ConfigureAwait(false);
                return obj;
            }
        }
        public static async Task<T[]> SelectAsync(NpgsqlConnection connection, string commandText, params NpgsqlParameter[] parameters)
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
        
        public static async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(NpgsqlConnection connection, string commandText, params NpgsqlParameter[] parameters)
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
        #endregion

        public static async Task<int> InsertAllAsync(NpgsqlConnection connection, T[] values, NpgsqlTransaction transaction = null)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            var parameters = new List<NpgsqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                names.Add($"( {String.Join(",", values[i].DBColumns.Select(c => "@" + i + c.Name))} )");
                parameters.AddRange(values[i].DBColumns.Select(c => values[i].CreateParameter(c, i.ToString())).ToArray());
            }

            string command = $"INSERT INTO {ObjectName} ({String.Join(",", values.First().DBColumns.Select(c => $"\"{c.Name}\""))}) VALUES {String.Join(",", names)}";


            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }
        
        public virtual NpgsqlParameter CreateParameter(ColumnInfo c)
        {
            var value = GetValue(c.Name);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new NpgsqlParameter(c.Name, c.Type) {Value = value, IsNullable = isNullable};
            return p;
        }

        public virtual NpgsqlParameter CreateParameter(ColumnInfo c, string namePrefix)
        {
            var value = GetValue(c.Name);

            var isNullable = IsNullable(value);

            if (isNullable && value == null)
            {
                value = DBNull.Value;
            }

            var p = new NpgsqlParameter(namePrefix + c.Name, c.Type) { Value = value, IsNullable = isNullable };
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
        
        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"\"{x.parameterName}\" {x.sortDirection.UpperName()}"));
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
