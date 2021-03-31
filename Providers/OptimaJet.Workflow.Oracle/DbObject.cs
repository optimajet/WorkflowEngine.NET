using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle
{
    public class ColumnInfo
    {
        public string Name;
        public OracleDbType Type = OracleDbType.NVarchar2;
        public bool IsKey = false;
        public int Size = 256;
        public bool IsVirtual = false;
    }

    public abstract class DbObject
    {
        public static string SchemaName { get; set; }

        public static async Task CommitAsync(OracleConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.CommandText = "COMMIT";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public abstract class DbObject<T> : DbObject where T : DbObject<T>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        public static string DbTableName;

        public static string ObjectName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(SchemaName))
                    return $"{SchemaName}.{DbTableName}";
                return $"{DbTableName}";
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
        public virtual async Task<int> InsertAsync (OracleConnection connection)
        {
            string command = String.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => c.Name.ToUpperInvariant())),
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => ":" + c.Name)));

            OracleParameter[] parameters = DBColumns.Select(c => new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync (OracleConnection connection)
        {
            string command = String.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name)),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name )));

            OracleParameter[] parameters = DBColumns.Select(c =>
                new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, parameters).ConfigureAwait(false);
        }

        public static async Task<T[]> SelectAllAsync(OracleConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }
        
        public static async Task<T[]> SelectAllWithPagingAsync(OracleConnection connection, List<(string parameterName,SortDirection sortDirection)> orderParameters, Paging paging)
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

        public static async Task<int> GetCountAsync(OracleConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT COUNT(*) FROM {ObjectName}")
                .ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        public static async Task<int> GetCountByAsync<TSelect>(OracleConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"p_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE {propertyName.ToUpperInvariant()} = :{paramName}");
            
            var param = new OracleParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(value) };
            
            var result = await ExecuteCommandScalarAsync(connection, selectText, param).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);

        }
        public static async Task<T[]> SelectByAsync<TSelect>(OracleConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"p_{propertyName}";
            
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE {propertyName.ToUpperInvariant()} = :{paramName}");
            
            var param = new OracleParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(value) };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<int> DeleteByAsync<TSelect>(OracleConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value)
        {
            var t = new T();
            string propertyName = (selectorLambda.Body as MemberExpression).Member.Name;
            string paramName = $"p_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE {propertyName.ToUpperInvariant()} = :{paramName}");
            
            var param = new OracleParameter(paramName, t.DBColumns.Find(x=>x.Name == propertyName).Type, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(value) };
            
            return await ExecuteCommandNonQueryAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<T[]> SelectByWithPagingAsync<TSelect, TSort>(OracleConnection connection, Expression<Func<T, TSelect>> selectorLambda, TSelect value, Expression<Func<T, TSort>> sortLambda, SortDirection sortDirection, Paging paging)
        {
            var t = new T();
            string selectorProperty = (selectorLambda.Body as MemberExpression).Member.Name;
            string sortProperty =(sortLambda.Body as MemberExpression).Member.Name;
            
            string paramName = $"p_{selectorProperty}";
            
            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }
            
            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE {selectorProperty.ToUpperInvariant()} = :{paramName} ORDER BY {sortProperty.ToUpperInvariant()} {sortDirection.UpperName().ToUpperInvariant()}{pagingText}");
            
            var param = new OracleParameter(paramName, t.DBColumns.Find(x=>x.Name == selectorProperty).Type, ParameterDirection.Input) { Value = ConvertToDbCompatibilityType(value) };
            
            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }
        
        public static async Task<T> SelectByKeyAsync(OracleConnection connection, object id)
        {
            var t = new T();

            ColumnInfo key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE {key.Name.ToUpperInvariant()} = :p_id";
            OracleParameter[] parameters = new[] {new OracleParameter("p_id", key.Type, ConvertToDbCompatibilityType(id), ParameterDirection.Input)};

            return (await SelectAsync(connection, selectText, parameters).ConfigureAwait(false)).FirstOrDefault();
        }

        public static async Task<int> DeleteAsync(OracleConnection connection, object id)
        {
            var t = new T();
            ColumnInfo key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE {key.Name.ToUpperInvariant()} = :p_id", new OracleParameter("p_id", key.Type, ConvertToDbCompatibilityType(id), ParameterDirection.Input))
                .ConfigureAwait(false);
        }

        public static async Task<int> ExecuteCommandNonQueryAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.BindByName = true;
            command.Parameters.AddRange(parameters);
            int cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return cnt;
        }
       
        public static async Task<object> ExecuteCommandScalarAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.BindByName = true;
            command.Parameters.AddRange(parameters);
            return await command.ExecuteScalarAsync().ConfigureAwait(false);
        }
        
        
        public async static Task<T[]> SelectAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.Connection = connection;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = commandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.BindByName = true;
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
        public async static Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.Connection = connection;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = commandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.BindByName = true;
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

        public static async Task<int> InsertAllAsync (OracleConnection connection, T[] values)
        {
            if (values.Length < 1)
            {
                return 0;
            }

            foreach(T value in values)
            {
                await value.InsertAsync(connection).ConfigureAwait(false);
            }

            return values.Length;
        }
        
        public static object ConvertToDbCompatibilityType<TValue>(TValue obj)
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
                orderParameters.Select(x => $"{x.parameterName.ToUpperInvariant()} {x.sortDirection.UpperName()}"));
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
