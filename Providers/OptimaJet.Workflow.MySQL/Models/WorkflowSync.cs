using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace OptimaJet.Workflow.MySQL.Models
{
    public class WorkflowSync : DbObject<WorkflowSync>
    {
        static WorkflowSync()
        {
            DbTableName = "workflowsync";
        }

        public WorkflowSync()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Name", IsKey = true, Type = MySqlDbType.VarString, Size = 450},
                new ColumnInfo {Name = "Lock", Type = MySqlDbType.Binary}
            });
        }

        public string Name { get; set; }
        public Guid Lock { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Name":
                    return Name;
                case "Lock":
                    return Lock.ToByteArray();
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Lock":
                    Lock = new Guid((byte[])value);
                    break;
                case "Name":
                    Name = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowSync GetByName(MySqlConnection connection, string name)
        {
            var selectText = String.Format("SELECT * FROM {0} WHERE `Name` = @name", DbTableName);
            var locks = Select(connection, selectText, new MySqlParameter("name", MySqlDbType.VarString) { Value = name });

            return locks.FirstOrDefault();
        }

        public static int UpdateLock(MySqlConnection connection, string name, Guid oldLock, Guid newLock, MySqlTransaction transaction = null)
        {
            var command = String.Format("UPDATE {0} SET `Lock` = @newlock WHERE `Name` = @name AND `Lock` = @oldlock", DbTableName);
            var p1 = new MySqlParameter("newlock", MySqlDbType.Binary) { Value = newLock.ToByteArray() };
            var p2 = new MySqlParameter("oldlock", MySqlDbType.Binary) { Value = oldLock.ToByteArray() };
            var p3 = new MySqlParameter("name", MySqlDbType.VarString) { Value = name };

            return ExecuteCommand(connection, command, transaction, p1, p2, p3);
        }

    }
}
