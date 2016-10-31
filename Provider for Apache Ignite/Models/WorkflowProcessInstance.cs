using System;
using System.Collections.Generic;
using System.Data;
using Apache.Ignite.Core.Binary;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowProcessInstance : DbObject<WorkflowProcessInstance>
    {
        static WorkflowProcessInstance()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = typeof(Guid)},
                new ColumnInfo {Name = "ActivityName"},
                new ColumnInfo {Name = "IsDeterminingParametersChanged", Type = typeof(bool)},
                new ColumnInfo {Name = "PreviousActivity"},
                new ColumnInfo {Name = "PreviousActivityForDirect"},
                new ColumnInfo {Name = "PreviousActivityForReverse"},
                new ColumnInfo {Name = "PreviousState"},
                new ColumnInfo {Name = "PreviousStateForDirect"},
                new ColumnInfo {Name = "PreviousStateForReverse"},
                new ColumnInfo {Name = "SchemeId", Type = typeof(Guid)},
                new ColumnInfo {Name = "StateName"},
                new ColumnInfo {Name = "ParentProcessId", Type = typeof(Guid)},
                new ColumnInfo {Name = "RootProcessId", Type = typeof(Guid)}
            });
        }

        public string ActivityName { get; set; }
        public Guid Id { get; set; }
        public bool IsDeterminingParametersChanged { get; set; }
        public string PreviousActivity { get; set; }
        public string PreviousActivityForDirect { get; set; }
        public string PreviousActivityForReverse { get; set; }
        public string PreviousState { get; set; }
        public string PreviousStateForDirect { get; set; }
        public string PreviousStateForReverse { get; set; }
        public Guid? SchemeId { get; set; }
        public string StateName { get; set; }
        public Guid? ParentProcessId { get; set; }
        public Guid RootProcessId { get; set; }
        public List<WorkflowProcessInstancePersistence> Persistence { get; set; }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
                case "ActivityName":
                    return ActivityName;
                case "IsDeterminingParametersChanged":
                    return IsDeterminingParametersChanged;
                case "PreviousActivity":
                    return PreviousActivity;
                case "PreviousActivityForDirect":
                    return PreviousActivityForDirect;
                case "PreviousActivityForReverse":
                    return PreviousActivityForReverse;
                case "PreviousState":
                    return PreviousState;
                case "PreviousStateForDirect":
                    return PreviousStateForDirect;
                case "PreviousStateForReverse":
                    return PreviousStateForReverse;
                case "SchemeId":
                    return SchemeId;
                case "StateName":
                    return StateName;
                case "ParentProcessId":
                    return ParentProcessId;
                case "RootProcessId":
                    return RootProcessId;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = (Guid) value;
                    break;
                case "ActivityName":
                    ActivityName = value as string;
                    break;
                case "IsDeterminingParametersChanged":
                    IsDeterminingParametersChanged = value.ToString() == "1";
                    break;
                case "PreviousActivity":
                    PreviousActivity = value as string;
                    break;
                case "PreviousActivityForDirect":
                    PreviousActivityForDirect = value as string;
                    break;
                case "PreviousActivityForReverse":
                    PreviousActivityForReverse = value as string;
                    break;
                case "PreviousState":
                    PreviousState = value as string;
                    break;
                case "PreviousStateForDirect":
                    PreviousStateForDirect = value as string;
                    break;
                case "PreviousStateForReverse":
                    PreviousStateForReverse = value as string;
                    break;
                case "SchemeId":
                    SchemeId = value as Guid?;
                    break;
                case "StateName":
                    StateName = value as string;
                    break;
                case "ParentProcessId":
                    ParentProcessId = value as Guid?;
                    break;
                case "RootProcessId":
                    RootProcessId = (Guid) value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        private static string PersistenceFieldName = "Persistence";
        public override void WriteBinary(IBinaryWriter writer)
        {
            base.WriteBinary(writer);

            writer.WriteString(PersistenceFieldName, JsonConvert.SerializeObject(Persistence));
        }

        public override void ReadBinary(IBinaryReader reader)
        {
            base.ReadBinary(reader);
            Persistence = JsonConvert.DeserializeObject<List<WorkflowProcessInstancePersistence>>(reader.ReadString(PersistenceFieldName));
            if (Persistence == null)
                Persistence = new List<WorkflowProcessInstancePersistence>();
        }
    }
}