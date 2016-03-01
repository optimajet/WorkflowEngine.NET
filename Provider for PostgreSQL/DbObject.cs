using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            get { return string.Format("\"{0}\".\"{1}\"", SchemaName, DbTableName); }
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
        public virtual int Insert(NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    ObjectName, 
                    String.Join(",", DBColumns.Select(c=> string.Format("\"{0}\"", c.Name) )),
                    String.Join(",", DBColumns.Select(c=> "@" + c.Name)));

                command.Parameters.AddRange(DBColumns.Select(CreateParameter).ToArray());
                command.CommandType = CommandType.Text;
                int cnt = command.ExecuteNonQuery();
                return cnt;
            }
        }

        public int Update(NpgsqlConnection connection)
        {
            string command = string.Format("UPDATE {0} SET {1} WHERE {2}",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))));

            var parameters = DBColumns.Select(CreateParameter).ToArray();

            return ExecuteCommand(connection, command, parameters);
            
        }

        public static T SelectByKey(NpgsqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", DbTableName));
            }

            string selectText = string.Format("SELECT * FROM {0} WHERE \"{1}\" = @p_id", ObjectName, key.Name);
            var pId = new NpgsqlParameter("p_id", key.Type) {Value = id};

            return Select(connection, selectText, pId).FirstOrDefault();
        }

        public static int Delete(NpgsqlConnection connection, object id, NpgsqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", DbTableName));

            var pId = new NpgsqlParameter("p_id", key.Type) {Value = id};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE \"{1}\" = @p_id", ObjectName, key.Name), transaction, pId);
        }

        public static int ExecuteCommand(NpgsqlConnection connection, string commandText,
          params NpgsqlParameter[] parameters)
        {
            return ExecuteCommand(connection, commandText, null, parameters);
        }

        public static int ExecuteCommand(NpgsqlConnection connection, string commandText,NpgsqlTransaction transaction = null, params NpgsqlParameter[] parameters)
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

        public static T[] Select(NpgsqlConnection connection, string commandText, params NpgsqlParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                using (var oda = new NpgsqlDataAdapter(command))
                {
                    oda.Fill(dt);
                }

                var res = new List<T>();

                foreach (DataRow row in dt.Rows)
                {
                    T item = new T();
                    foreach (var c in item.DBColumns)
                        item.SetValue(c.Name, row[c.Name]);
                    res.Add(item);
                }

                return res.ToArray();
            }
        }
        #endregion

        public virtual NpgsqlParameter CreateParameter(ColumnInfo c)
        {
            var p = new NpgsqlParameter(c.Name, c.Type) {Value = GetValue(c.Name)};
            return p;
        }
    }
}
