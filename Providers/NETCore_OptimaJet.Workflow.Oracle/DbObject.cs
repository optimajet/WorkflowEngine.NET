using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public virtual int Insert(OracleConnection connection)
        {
            string command = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => c.Name.ToUpperInvariant())),
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => ":" + c.Name)));

            var parameters = DBColumns.Select(c => new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return ExecuteCommand(connection, command, parameters);
        }

        public int Update(OracleConnection connection)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name)),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name )));

            var parameters = DBColumns.Select(c =>
                new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return ExecuteCommand(connection, command, parameters);
            
        }

        public static T SelectByKey(OracleConnection connection, object id)
        {
            var t = new T();

            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));
            }

            string selectText = string.Format("SELECT * FROM {0} WHERE {1} = :p_id", ObjectName, key.Name.ToUpperInvariant());
            var parameters = new[] {new OracleParameter("p_id", key.Type, ConvertToDBCompatibilityType(id), ParameterDirection.Input)};

            return Select(connection, selectText, parameters).FirstOrDefault();
        }

        public static int Delete(OracleConnection connection, object id)
        {
            var t = new T();
            var key = t.DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE {1} = :p_id", ObjectName, key.Name.ToUpperInvariant()),
                new OracleParameter[]{
                    new OracleParameter("p_id", key.Type, ConvertToDBCompatibilityType(id), ParameterDirection.Input)});
        }

        public static int ExecuteCommand(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.BindByName = true;
                command.Parameters.AddRange(parameters);
                var cnt = command.ExecuteNonQuery();
                return cnt;
            }
        }

        public static T[] Select(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;
                command.BindByName = true;
                command.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                using (var oda = new OracleDataAdapter(command))
                {
                    oda.Fill(dt);
                }

                var res = new List<T>();

                foreach (DataRow row in dt.Rows)
                {
                    T item = new T();
                    foreach (var c in item.DBColumns)
                        item.SetValue(c.Name, row[c.Name.ToUpperInvariant()]);
                    res.Add(item);
                }

                return res.ToArray();
            }
        }

        public static void Commit(OracleConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "COMMIT";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        #endregion

        public static object ConvertToDBCompatibilityType(object obj)
        {
            if (obj is Guid)
                return ((Guid)obj).ToByteArray();
            return obj;
        }
    }
}
