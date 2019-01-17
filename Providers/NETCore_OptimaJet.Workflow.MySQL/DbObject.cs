using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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
        public virtual int Insert(MySqlConnection connection)
        {
            string command = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    DbTableName,
                    String.Join(",", DBColumns.Select(c => string.Format("`{0}`", c.Name))),
                    String.Join(",", DBColumns.Select(c => "@" + c.Name)));

            var parameters = DBColumns.Select(c => CreateParameter(c)).ToArray();
            return ExecuteCommand(connection, command, parameters);
        }

        public int Update(MySqlConnection connection)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    DbTableName,
                    String.Join(",", DBColumns.Where(c => !c.IsKey).Select(c => string.Format("`{0}` = @{0}", c.Name))),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => string.Format("`{0}` = @{0}", c.Name))));

            var parameters = DBColumns.Select(c =>
                CreateParameter(c)).ToArray();

            return ExecuteCommand(connection, command, parameters);
            
        }

        public static T SelectByKey(MySqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", DbTableName));
            }

            string selectText = string.Format("SELECT * FROM {0} WHERE `{1}` = @p_id", DbTableName, key.Name);
            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDBCompatibilityType(id)};

            return Select(connection, selectText, pId).FirstOrDefault();
        }

        public static int Delete(MySqlConnection connection, object id, MySqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", DbTableName));

            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDBCompatibilityType(id)};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE `{1}` = @p_id", DbTableName, key.Name), transaction, pId);
        }
        public static int ExecuteCommand(MySqlConnection connection, string commandText,
            params MySqlParameter[] parameters)
        {
            return ExecuteCommand(connection, commandText, null, parameters);
        }

        public static int ExecuteCommand(MySqlConnection connection, string commandText, MySqlTransaction transaction = null, params MySqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
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
                var cnt = command.ExecuteNonQuery();
                return cnt;
            }
        }

        public static T[] Select(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            return SelectAsync(connection, commandText, parameters).Result;
        }

        public static async Task<T[]> SelectAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
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

        public static object ConvertToDBCompatibilityType(object obj)
        {
            if (obj is Guid)
                return ((Guid)obj).ToByteArray();
            return obj;
        }

        public virtual MySqlParameter CreateParameter(ColumnInfo c)
        {
            var p = new MySqlParameter(c.Name, c.Type) {Value = GetValue(c.Name)};
            return p;
        }
    }
}
