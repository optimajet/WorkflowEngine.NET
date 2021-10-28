using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.MySQL
{
    public class ProcessInstanceTreeItem : DbObject<ProcessInstanceTreeItem>, IProcessInstanceTreeItem
    {
        static ProcessInstanceTreeItem()
        {
            DbTableName = "virtual"; //this entity is combination of WorkflowProcessInstance and WorkflowProcessScheme
        }

        public ProcessInstanceTreeItem()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = "ParentProcessId", Type =  MySqlDbType.Binary},
                new ColumnInfo {Name = "RootProcessId", Type =  MySqlDbType.Binary},
                new ColumnInfo {Name = "StartingTransition"},
                new ColumnInfo {Name = nameof(SubprocessName)}
            });
        }

        public Guid Id { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public string SubprocessName { get; set; }
        public string StartingTransition { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "ParentProcessId":
                    return ParentProcessId?.ToByteArray();
                case "RootProcessId":
                    return RootProcessId.ToByteArray();
                case "StartingTransition":
                    return StartingTransition;
                case nameof(SubprocessName):
                    return SubprocessName;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                {
                    Id = (value is byte[] bytes) ? new Guid(bytes) : throw new Exception($"DB mapping error. {nameof(Id)} can't be null");
                    break;
                }
                case "ParentProcessId":
                {
                    ParentProcessId = (value is byte[] bytes) ? new Guid(bytes) : (Guid?)null;
                    break;
                }
                case "RootProcessId":
                {
                    RootProcessId = (value is byte[] bytes) ? new Guid(bytes) : throw new Exception($"DB mapping error. {nameof(RootProcessId)} can't be null");
                    break;
                }
                case "StartingTransition":
                    StartingTransition = value as string;
                    break;
                case nameof(SubprocessName):
                    SubprocessName = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        
        public static async Task<List<IProcessInstanceTreeItem>> GetProcessTreeItemsByRootProcessId(MySqlConnection connection, Guid rootProcessId)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");
            builder.Append($"`{nameof(Id)}`, `{nameof(ParentProcessId)}`, `{nameof(RootProcessId)}`, `{nameof(StartingTransition)}`, `{nameof(SubprocessName)}` ");
            builder.Append($"FROM {WorkflowProcessInstance.DbTableName} ");
            builder.Append($"WHERE `{nameof(RootProcessId)}` = @rootProcessId");

            return (await SelectAsync(connection, builder.ToString(), new MySqlParameter("rootProcessId", MySqlDbType.Binary) {Value = rootProcessId.ToByteArray()})
                .ConfigureAwait(false)).Cast<IProcessInstanceTreeItem>().ToList();
        }
    }
}
