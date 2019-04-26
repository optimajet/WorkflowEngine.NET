using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }
        public bool CanBeInlined { get; set; }
        public string InlinedSchemes { get; set; }

        static WorkflowScheme()
        {
            DbTableName = "workflowscheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Code", IsKey = true},
                new ColumnInfo {Name = "Scheme", Type = MySqlDbType.LongText},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = MySqlDbType.Bit},
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
                    CanBeInlined = value.ToString() == "1";
                    break;
                case nameof(InlinedSchemes):
                    InlinedSchemes = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }
        
        public static List<string> GetInlinedSchemeCodes(MySqlConnection connection)
        {
            var selectText = $"SELECT * FROM {DbTableName} WHERE `CanBeInlined` = 1";
            var schemes = Select(connection, selectText);
            return schemes.Select(sch => sch.Code).ToList();
        }
        
        public static List<string> GetRelatedSchemeCodes(MySqlConnection connection, string schemeCode)
        {
            var selectText =  $"SELECT * FROM {DbTableName} WHERE `{nameof(InlinedSchemes)}` LIKE CONCAT('%',@search,'%')";
            var p = new MySqlParameter("search", MySqlDbType.VarString) {Value = $"\"{schemeCode}\""};
            return Select(connection, selectText, p).Select(sch=>sch.Code).Distinct().ToList();
        }
    }
}
