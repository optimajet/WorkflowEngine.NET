using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

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

        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }
      
        #region Command Insert/Update/Delete/Commit
        
        public int Insert(NpgsqlConnection connection)
        {
            return InsertAsync(connection).Result;
        }
        
        public Task<int> InsertAsync(NpgsqlConnection connection)
        {
            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                ObjectName,
                String.Join(",", DBColumns.Where(c=> !c.IsVirtual).Select(c => string.Format("\"{0}\"", c.Name))),
                String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name)));
            var parameters = DBColumns.Select(CreateParameter).ToArray();
            return ExecuteCommandAsync(connection, commandText, parameters);
        }

        public virtual int Update(NpgsqlConnection connection)
        {
            return UpdateAsync(connection).Result;
        }
        
        public virtual Task<int> UpdateAsync(NpgsqlConnection connection)
        {
            string command = string.Format("UPDATE {0} SET {1} WHERE {2}",
                ObjectName,
                String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))),
                String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))));

            var parameters = DBColumns.Select(CreateParameter).ToArray();

            return ExecuteCommandAsync(connection, command, parameters);
          
        }

        public static T[] SelectAll(NpgsqlConnection connection)
        {
            return Select(connection, string.Format("SELECT * FROM {0}", ObjectName));
        }

        public static T SelectByKey(NpgsqlConnection connection, object id)
        {
            return SelectByKeyAsync(connection, id).Result;
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

        public static int Delete(NpgsqlConnection connection, object id, NpgsqlTransaction transaction = null)
        {
            return DeleteAsync(connection, id, transaction).Result;
        }
        
        public static Task<int> DeleteAsync(NpgsqlConnection connection, object id, NpgsqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception($"Key for table {DbTableName} isn't defined.");

            var pId = new NpgsqlParameter("p_id", key.Type) {Value = id};

            return ExecuteCommandAsync(connection, $"DELETE FROM {ObjectName} WHERE \"{key.Name}\" = @p_id", transaction, pId);
        }

        public static int ExecuteCommand(NpgsqlConnection connection, string commandText,
          params NpgsqlParameter[] parameters)
        {
            return ExecuteCommand(connection, commandText, null, parameters);
        }

        public static Task<int> ExecuteCommandAsync(NpgsqlConnection connection, string commandText,
            params NpgsqlParameter[] parameters)
        {
            return ExecuteCommandAsync(connection, commandText, null, parameters);
        }
        
        public static int ExecuteCommand(NpgsqlConnection connection, string commandText,NpgsqlTransaction transaction = null, params NpgsqlParameter[] parameters)
        {
            return ExecuteCommandAsync(connection, commandText, transaction, parameters).Result;
        }

        public static async Task<int> ExecuteCommandAsync(NpgsqlConnection connection, string commandText, NpgsqlTransaction transaction = null,
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
        
        public static T[] Select(NpgsqlConnection connection, string commandText, params NpgsqlParameter[] parameters)
        {
            return SelectAsync(connection, commandText, parameters).Result;
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
        #endregion

        public static async Task<int> InsertAllAsync(NpgsqlConnection connection, T[] values)
        {
            if (values.Length < 1)
                return 0;

            var parameters = new List<NpgsqlParameter>();
            var names = new List<string>();

            for (int i = 0; i < values.Length; i++)
            {
                names.Add(String.Join(",", values[i].DBColumns.Select(c => "@" + i.ToString() + c.Name)));
                parameters.AddRange(values[i].DBColumns.Select(c => values[i].CreateParameter(c, i.ToString())).ToArray());
            }

            string command = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                ObjectName,
                String.Join(",", values.First().DBColumns.Select(c => string.Format("\"{0}\"", c.Name))),
                String.Join(",", names));


            return await ExecuteCommandAsync(connection, command, parameters.ToArray()).ConfigureAwait(false);
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
    }
}
