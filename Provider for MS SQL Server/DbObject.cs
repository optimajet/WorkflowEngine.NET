using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;

namespace OptimaJet.Workflow.DbPersistence
{
    public class ColumnInfo
    {
        public string Name;
        public SqlDbType Type = SqlDbType.NVarChar;
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
        public virtual int Insert(SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    DbTableName, 
                    String.Join(",", DbColumns.Select(c=> string.Format("[{0}]", c.Name) )),
                    String.Join(",", DbColumns.Select(c=> "@" + c.Name)));

                command.Parameters.AddRange(DbColumns.Select(CreateParameter).ToArray());
                command.CommandType = CommandType.Text;
                int cnt = command.ExecuteNonQuery();
                return cnt;
            }
        }

        public int Update(SqlConnection connection)
        {
            string command = string.Format(@"UPDATE {0} SET {1} WHERE {2}",
                    DbTableName,
                    String.Join(",", DbColumns.Where(c => !c.IsKey).Select(c => string.Format("[{0}] = @{0}", c.Name))),
                    String.Join(" AND ", DbColumns.Where(c => c.IsKey).Select(c => string.Format("[{0}] = @{0}", c.Name))));

            var parameters = DbColumns.Select(CreateParameter).ToArray();

            return ExecuteCommand(connection, command, parameters);
            
        }

        public static T SelectByKey(SqlConnection connection, object id)
        {
            var t = new T();

            var key = t.DbColumns.FirstOrDefault(c => c.IsKey);
            if(key == null)
            {
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));
            }

            string selectText = string.Format("SELECT * FROM {0} WHERE [{1}] = @p_id", ObjectName, key.Name);
            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return Select(connection, selectText, pId).FirstOrDefault();
        }

        public static int Delete(SqlConnection connection, object id, SqlTransaction transaction = null)
        {
            var t = new T();
            var key = t.DbColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
                throw new Exception(string.Format("Key for table {0} isn't defined.", ObjectName));

            var pId = new SqlParameter("p_id", key.Type) {Value = ConvertToDbCompatibilityType(id)};

            return ExecuteCommand(connection,
                string.Format("DELETE FROM {0} WHERE [{1}] = @p_id", ObjectName, key.Name), transaction, pId);
        }

        public static int ExecuteCommand(SqlConnection connection, string commandText, 
            params SqlParameter[] parameters)
        {
            return ExecuteCommand(connection, commandText, null, parameters);
        }

        public static int ExecuteCommand(SqlConnection connection, string commandText, SqlTransaction transaction = null, params SqlParameter[] parameters)
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

        public static T[] Select(SqlConnection connection, string commandText, params SqlParameter[] parameters)
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
                var res = new List<T>();
#if NETCOREAPP
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
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
#else
                DataTable dt = new DataTable();
                using (var oda = new SqlDataAdapter(command))
                {
                    oda.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    T item = new T();
                    foreach (var c in item.DbColumns)
                        item.SetValue(c.Name, row[c.Name]);
                    res.Add(item);
                }
#endif
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
            if (obj == null) return true; 
            Type type = typeof (T1);
            if (!type.GetTypeInfo().IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true; 
            return false;
        }
    }
}
