using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
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
                if (!string.IsNullOrWhiteSpace(SchemaName))
                    return string.Format("{0}.{1}", SchemaName, DbTableName);
                return string.Format("{0}",DbTableName);
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

            return await ExecuteCommandAsync(connection, command, parameters).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync (OracleConnection connection)
        {
            string command = String.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name)),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name )));

            OracleParameter[] parameters = DBColumns.Select(c =>
                new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return await ExecuteCommandAsync(connection, command, parameters).ConfigureAwait(false);
        }

        public static async Task<T[]> SelectAllAsync(OracleConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
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
            OracleParameter[] parameters = new[] {new OracleParameter("p_id", key.Type, ConvertToDBCompatibilityType(id), ParameterDirection.Input)};

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

            return await ExecuteCommandAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE {key.Name.ToUpperInvariant()} = :p_id", new OracleParameter("p_id", key.Type, ConvertToDBCompatibilityType(id), ParameterDirection.Input))
                .ConfigureAwait(false);
        }

        public static async Task<int> ExecuteCommandAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
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
        
        public static object ConvertToDBCompatibilityType(object obj)
        {
            if (obj is Guid guid)
            {
                return guid.ToByteArray();
            }

            return obj;
        }
    }
}
