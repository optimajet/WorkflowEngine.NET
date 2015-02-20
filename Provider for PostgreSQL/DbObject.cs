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
    public class DbObject<T> where T : DbObject<T>, new()
    {
        public DbObject()
        {

        }

        public string db_TableName;
        public List<ColumnInfo> db_Columns = new List<ColumnInfo>();

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
                command.CommandText = string.Format("INSERT INTO \"{0}\" ({1}) VALUES ({2})",
                    db_TableName, 
                    String.Join(",", db_Columns.Select(c=> string.Format("\"{0}\"", c.Name) )),
                    String.Join(",", db_Columns.Select(c=> "@" + c.Name)));

                command.Parameters.AddRange(db_Columns.Select(c=> CreateParameter(c)).ToArray());
                command.CommandType = CommandType.Text;
                int cnt = command.ExecuteNonQuery();
                return cnt;
            }
        }

        public int Update(NpgsqlConnection connection)
        {
            string command = string.Format("UPDATE \"{0}\" SET {1} WHERE {2}",
                    db_TableName,
                    String.Join(",", db_Columns.Where(c => !c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))),
                    String.Join(" AND ", db_Columns.Where(c => c.IsKey).Select(c => string.Format("\"{0}\" = @{0}", c.Name))));

            var parameters = db_Columns.Select(c =>
                CreateParameter(c)).ToArray();

            return ExecuteCommand(connection, command, parameters);
            
        }

        public static T SelectByKey(NpgsqlConnection connection, object id)
        {
            var t = new T();

            var key = t.db_Columns.Where(c => c.IsKey).FirstOrDefault();
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", t.db_TableName));
            }

            string selectText = string.Format("SELECT * FROM \"{0}\" WHERE \"{1}\" = @p_id", t.db_TableName, key.Name);
            var p_id = new NpgsqlParameter("p_id", key.Type);
            p_id.Value = id;

            return Select(connection, selectText, p_id).FirstOrDefault();
        }

        public static int Delete(NpgsqlConnection connection, object id)
        {
            var t = new T();
            var key = t.db_Columns.Where(c => c.IsKey).FirstOrDefault();
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", t.db_TableName));

            var p_id = new NpgsqlParameter("p_id", key.Type);
            p_id.Value = id;

            return ExecuteCommand(connection,
                string.Format("DELETE FROM \"{0}\" WHERE \"{1}\" = @p_id", t.db_TableName, key.Name), p_id);
        }

        public static int ExecuteCommand(NpgsqlConnection connection, string commandText, params NpgsqlParameter[] parameters)
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
                    foreach (var c in item.db_Columns)
                        item.SetValue(c.Name, row[c.Name]);
                    res.Add(item);
                }

                return res.ToArray();
            }
        }
        #endregion

        public virtual NpgsqlParameter CreateParameter(ColumnInfo c)
        {
            var p = new NpgsqlParameter(c.Name, c.Type);
            p.Value = GetValue(c.Name);
            return p;
        }
    }
}
