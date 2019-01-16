using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

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
                if (!string.IsNullOrWhiteSpace(SchemaName))
                    return string.Format("[{0}].[{1}]", SchemaName, DbTableName);
                return string.Format("[{0}]", DbTableName);
            }
        }

        public List<ColumnInfo> DbColumns = new List<ColumnInfo>();

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }
      
        #region Command Insert/Update/Delete/Commit
        
        public int Insert(SqlConnection connection)
        {
            return InsertAsync(connection).Result;
        }
        
        public Task<int> InsertAsync(SqlConnection connection)
        {
            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                ObjectName,
                String.Join(",", DbColumns.Where(c=> !c.IsVirtual).Select(c => string.Format("[{0}]", c.Name))),
                String.Join(",", DbColumns.Where(c => !c.IsVirtual).Select(c => "@" + c.Name)));
            var parameters = DbColumns.Select(CreateParameter).ToArray();
            return ExecuteCommandAsync(connection, commandText, parameters);
        }

        public int Update(SqlConnection connection)
        {
            return UpdateAsync(connection).Result;
        }
        
        public Task<int> UpdateAsync(SqlConnection connection)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                ObjectName,
                String.Join(",", DbColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => string.Format("[{0}] = @{0}", c.Name))),
                String.Join(" AND ", DbColumns.Where(c => c.IsKey).Select(c => string.Format("[{0}] = @{0}", c.Name))));

            var parameters = DbColumns.Select(CreateParameter).ToArray();

            return ExecuteCommandAsync(connection, command, parameters);
            
        }

        public static T SelectByKey(SqlConnection connection, object id)
        {
            return SelectByKeyAsync(connection, id).Result;
        }
        
        public static async Task<T> SelectByKeyAsync(SqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DbColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));
            }

            string selectText = string.Format("SELECT * FROM {0} WHERE [{1}] = @p_id", ObjectName, key.Name);
            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return (await SelectAsync(connection, selectText, pId).ConfigureAwait(false)).FirstOrDefault();
        }

        public static int Delete(SqlConnection connection, object id, SqlTransaction transaction = null)
        {
            return DeleteAsync(connection, id, transaction).Result;
        }
        
        public static Task<int> DeleteAsync(SqlConnection connection, object id, SqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DbColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));

            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return ExecuteCommandAsync(connection,
                string.Format("DELETE FROM {0} WHERE [{1}] = @p_id", ObjectName, key.Name), transaction, pId);
        }


        public static int ExecuteCommand(SqlConnection connection, string commandText, 
            params SqlParameter[] parameters)
        {
            return ExecuteCommand(connection, commandText, null, parameters);
        }
        
        public static Task<int> ExecuteCommandAsync(SqlConnection connection, string commandText, 
            params SqlParameter[] parameters)
        {
            return ExecuteCommandAsync(connection, commandText, null, parameters);
        }

        public static int ExecuteCommand(SqlConnection connection, string commandText, SqlTransaction transaction = null, params SqlParameter[] parameters)
        {
            return ExecuteCommandAsync(connection, commandText, transaction, parameters).Result;
        }
        
        public static async Task<int> ExecuteCommandAsync(SqlConnection connection, string commandText, SqlTransaction transaction = null, params SqlParameter[] parameters)
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


        public static T[] Select(SqlConnection connection, string commandText, params SqlParameter[] parameters)
        {
            return SelectAsync(connection, commandText, parameters).Result;
        }
        
        public static async Task<T[]> SelectAsync(SqlConnection connection, string commandText, params SqlParameter[] parameters)
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
                            var column = item.DbColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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

        private static bool IsNullable<T1>(T1 obj)
        {
            if (obj == null) return true;  //-V3111
            Type type = typeof (T1);
            if (!type.GetTypeInfo().IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true; 
            return false;
        }
    }
}
