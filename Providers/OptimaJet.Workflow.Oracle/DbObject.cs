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

        public static void Commit(OracleConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using OracleCommand command = connection.CreateCommand();
            command.CommandText = "COMMIT";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
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
        public virtual int Insert(OracleConnection connection)
        {
            string command = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => c.Name.ToUpperInvariant())),
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => ":" + c.Name)));

            var parameters = DBColumns.Select(c => new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return ExecuteCommand(connection, command, parameters);
        }

        public virtual int Update(OracleConnection connection)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    ObjectName,
                    String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name)),
                    String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name )));

            var parameters = DBColumns.Select(c =>
                new OracleParameter(c.Name, c.Type, GetValue(c.Name), ParameterDirection.Input)).ToArray();

            return ExecuteCommand(connection, command, parameters);
        }

        public static T[] SelectAll(OracleConnection connection)
        {
            return Select(connection, string.Format("SELECT * FROM {0}", ObjectName));
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
            return SelectAsync(connection, commandText, parameters).Result;
        }
        
        public  async static Task<T[]> SelectAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using (OracleCommand command = connection.CreateCommand())
            {
                command.Connection = connection;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = commandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                command.BindByName = true;
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(parameters);
                var res = new List<T>();
                using (DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var item = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string name = reader.GetName(i);
                            ColumnInfo column = item.DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
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

        public static int InsertAll (OracleConnection connection, T[] values)
        {
            if (values.Length < 1)
                return 0;

            foreach(var value in values)
            {
                value.Insert(connection);
            }

            return values.Length;
        }
        
        public static object ConvertToDBCompatibilityType(object obj)
        {
            if (obj is Guid)
                return ((Guid)obj).ToByteArray();
            return obj;
        }
    }
}
