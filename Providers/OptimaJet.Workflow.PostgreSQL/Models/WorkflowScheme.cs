using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }
        public bool CanBeInlined { get; set; }
        public string InlinedSchemes { get; set; }

        static WorkflowScheme()
        {
            DbTableName = "WorkflowScheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Code", IsKey = true},
                new ColumnInfo {Name="Scheme", Type = NpgsqlDbType.Text},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(InlinedSchemes)}
            });
        }

        public override object GetValue(string key)
        {
            switch(key)
            {
                case "Code":
                    return Code;
                case "Scheme":
                    return Scheme;
                case nameof(CanBeInlined):
                    return CanBeInlined;
                case nameof(InlinedSchemes):
                    return InlinedSchemes;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Code":
                    Code = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                case nameof(CanBeInlined):
                    CanBeInlined = (bool) value;
                    break;
                case nameof(InlinedSchemes):
                    InlinedSchemes = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }
        
        public static List<string> GetInlinedSchemeCodes(NpgsqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE \"{nameof(CanBeInlined)}\" = TRUE";
            var schemes = Select(connection, selectText);
            return schemes.Select(sch => sch.Code).ToList();
        }
        
        public static List<string> GetRelatedSchemeCodes(NpgsqlConnection connection, string schemeCode)
        {
            var selectText =  $"SELECT * FROM {ObjectName} WHERE \"{nameof(InlinedSchemes)}\" LIKE '%' || @search || '%'";
            var p = new NpgsqlParameter("search", NpgsqlDbType.Varchar) {Value = $"\"{schemeCode}\""};
            return Select(connection, selectText, p).Select(sch=>sch.Code).Distinct().ToList();
        }
    }
}
