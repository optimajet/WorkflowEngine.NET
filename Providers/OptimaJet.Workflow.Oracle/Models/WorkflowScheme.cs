using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }
        public bool CanBeInlined { get; set; }
        public string InlinedSchemes { get; set; }

        static WorkflowScheme ()
        {
            DbTableName = "WorkflowScheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Code", IsKey = true},
                new ColumnInfo {Name="Scheme", Type = OracleDbType.Clob},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = OracleDbType.Byte},
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
                    return CanBeInlined ? "1" : "0";
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
                    CanBeInlined = (string)value == "1";
                    break;
                case nameof(InlinedSchemes):
                    InlinedSchemes = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }
        
        public static List<string> GetInlinedSchemeCodes(OracleConnection connection)
        {
            var selectText = $"SELECT * FROM {DbTableName} WHERE {nameof(CanBeInlined).ToUpper()} = 1";
            var schemes = Select(connection, selectText).ToList();
            return schemes.Select(sch => sch.Code).ToList();
        }

        public static List<string> GetRelatedSchemeCodes(OracleConnection connection, string schemeCode)
        {
            var selectText = $"SELECT * FROM {DbTableName} WHERE {nameof(InlinedSchemes).ToUpper()} LIKE '%' || :search || '%'";
            var p = new OracleParameter("search", OracleDbType.NVarchar2, $"\"{schemeCode}\"", ParameterDirection.Input);
            return Select(connection, selectText, p).Select(sch => sch.Code).ToList();
        }
    }
}
