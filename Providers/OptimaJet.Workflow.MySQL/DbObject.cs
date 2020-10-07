using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        public virtual async Task<int> InsertAsync(MySqlConnection connection, MySqlTransaction transaction = null)
        {
            string command = $"INSERT INTO {DbTableName} ({String.Join(",", DBColumns.Select(c => $"`{c.Name}`"))}) VALUES ({String.Join(",", DBColumns.Select(c => "@" + c.Name))})";

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(c)).ToArray();
            return await ExecuteCommandAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync(MySqlConnection connection, MySqlTransaction transaction = null)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    DbTableName,
                    String.Join(",", DBColumns.Where(c => !c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name))),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => String.Format("`{0}` = @{0}", c.Name))));

            MySqlParameter[] parameters = DBColumns.Select(c => CreateParameter(c)).ToArray();

            return await ExecuteCommandAsync(connection, command, transaction, parameters).ConfigureAwait(false);
            
        }

        public static async Task<T[]> SelectAllAsync(MySqlConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {DbTableName}").ConfigureAwait(false);
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
            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDBCompatibilityType(id)};

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

            var pId = new MySqlParameter("p_id", key.Type) {Value = ConvertToDBCompatibilityType(id)};

            return await ExecuteCommandAsync(connection,
                $"DELETE FROM {DbTableName} WHERE `{key.Name}` = @p_id", transaction, pId).ConfigureAwait(false);
        }

        public static Task<int> ExecuteCommandAsync(MySqlConnection connection, string commandText, params MySqlParameter[] parameters)
        {
            return ExecuteCommandAsync(connection, commandText, null, parameters);
        }

        public static async Task<int> ExecuteCommandAsync(MySqlConnection connection, string commandText, MySqlTransaction transaction = null, params MySqlParameter[] parameters)
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
                names.Add(String.Join(",", values[i].DBColumns.Select(c => "@" + i + c.Name)));
                parameters.AddRange(values[i].DBColumns.Select(c => values[i].CreateParameter(c, i.ToString())).ToArray());
            }

            string command = $"INSERT INTO {DbTableName} ({String.Join(",", values.First().DBColumns.Select(c => $"`{c.Name}`"))}) VALUES ({String.Join(",", names)})";

            return await ExecuteCommandAsync(connection, command, transaction, parameters.ToArray()).ConfigureAwait(false);
        }

        public static object ConvertToDBCompatibilityType(object obj)
        {
            if (obj is Guid guid)
            {
                return guid.ToByteArray();
            }

            return obj;
        }

        public virtual MySqlParameter CreateParameter(ColumnInfo c, string namePrefix = "")
        {
            var p = new MySqlParameter(namePrefix+c.Name, c.Type) {Value = GetValue(c.Name)};
            return p;
        }
    }
}
