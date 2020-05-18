using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace OptimaJet.Workflow.PostgreSQL.Models
{
    public class WorkflowSync : DbObject<WorkflowSync>
    {
        static WorkflowSync()
        {
            DbTableName = "WorkflowSync";
        }

        public WorkflowSync()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Name", IsKey = true},
                new ColumnInfo {Name = "Lock", Type = NpgsqlDbType.Uuid}
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
                    return Lock;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Lock":
                    Lock = (Guid)value;
                    break;
                case "Name":
                    Name = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowSync GetByName(NpgsqlConnection connection, string name)
        {
            var selectText = String.Format("SELECT * FROM {0} WHERE \"Name\" = @name", ObjectName);
            var locks = Select(connection, selectText, new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name });

            return locks.FirstOrDefault();
        }

        public static int UpdateLock(NpgsqlConnection connection, string name, Guid oldLock, Guid newLock, NpgsqlTransaction transaction = null)
        {
            var command = String.Format("UPDATE {0} SET \"Lock\" = @newlock WHERE \"Name\" = @name AND \"Lock\" = @oldlock", ObjectName);
            var p1 = new NpgsqlParameter("newlock", NpgsqlDbType.Uuid) { Value = newLock };
            var p2 = new NpgsqlParameter("oldlock", NpgsqlDbType.Uuid) { Value = oldLock };
            var p3 = new NpgsqlParameter("name", NpgsqlDbType.Varchar) { Value = name };

            return ExecuteCommand(connection, command, transaction, p1, p2, p3);
        }

    }
}
