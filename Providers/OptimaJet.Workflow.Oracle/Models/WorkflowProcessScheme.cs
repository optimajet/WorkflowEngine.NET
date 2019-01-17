using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessScheme : DbObject<WorkflowProcessScheme>
    {
        public string DefiningParameters { get; set; }
        public string DefiningParametersHash { get; set; }
        public Guid Id { get; set; }
        public bool IsObsolete { get; set; }
        public string SchemeCode { get; set; }
        public string Scheme { get; set; }
        public Guid? RootSchemeId { get; set; }
        public string RootSchemeCode { get; set; }
        public string AllowedActivities { get; set; }
        public string StartingTransition { get; set; }

        static WorkflowProcessScheme()
        {
            DbTableName = "WorkflowProcessScheme";
        }

        public WorkflowProcessScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = "DefiningParameters"},
                new ColumnInfo {Name = "DefiningParametersHash"},
                new ColumnInfo {Name = "IsObsolete", Type = OracleDbType.Byte},
                new ColumnInfo {Name = "SchemeCode"},
                new ColumnInfo {Name = "Scheme"},
                new ColumnInfo {Name = "RootSchemeId", Type = OracleDbType.Raw},
                new ColumnInfo {Name = "RootSchemeCode"},
                new ColumnInfo {Name = "AllowedActivities"},
                new ColumnInfo {Name = "StartingTransition"}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "DefiningParameters":
                    return DefiningParameters;
                case "DefiningParametersHash":
                    return DefiningParametersHash;
                case "IsObsolete":
                    return IsObsolete ? "1" : "0";
                case "SchemeCode":
                    return SchemeCode;
                case "Scheme":
                    return Scheme;
                case "RootSchemeId":
                    return RootSchemeId.HasValue ? RootSchemeId.Value.ToByteArray() : null;
                case "RootSchemeCode":
                    return RootSchemeCode;
                case "AllowedActivities":
                    return AllowedActivities;
                case "StartingTransition":
                    return StartingTransition;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = new Guid((byte[]) value);
                    break;
                case "DefiningParameters":
                    DefiningParameters = value as string;
                    break;
                case "DefiningParametersHash":
                    DefiningParametersHash = value as string;
                    break;
                case "IsObsolete":
                    IsObsolete = (string) value == "1";
                    break;
                case "SchemeCode":
                    SchemeCode = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                case "RootSchemeId":
                    var bytes1 = value as byte[];
                    if (bytes1 != null)
                        RootSchemeId = new Guid(bytes1);
                    else
                        RootSchemeId = null;
                    break;
                case "RootSchemeCode":
                    RootSchemeCode = value as string;
                    break;
                case "AllowedActivities":
                    AllowedActivities = value as string;
                    break;
                case "StartingTransition":
                    StartingTransition = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowProcessScheme[] Select(OracleConnection connection, string schemeCode,
            string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText =
                string.Format("SELECT * FROM {0}  WHERE SchemeCode = :schemecode AND DefiningParametersHash = :dphash",
                    ObjectName);

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += " AND ISOBSOLETE = 1";
                }
                else
                {
                    selectText += " AND ISOBSOLETE = 0";
                }
            }

            if (rootSchemeId.HasValue)
            {
                selectText += " AND ROOTSCHEMEID = :rootschemeid";
                return Select(connection, selectText,
                    new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                    new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash,
                        ParameterDirection.Input),
                    new OracleParameter("rootschemeid", OracleDbType.Raw, rootSchemeId.Value.ToByteArray(), ParameterDirection.Input));
            }
            else
            {
                selectText += " AND ROOTSCHEMEID IS NULL";
                return Select(connection, selectText,
                    new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                    new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash,
                        ParameterDirection.Input));
            }

        }

        public static int SetObsolete(OracleConnection connection, string schemeCode)
        {
            string command = string.Format("UPDATE {0} SET IsObsolete = 1 WHERE SchemeCode = :schemecode OR RootSchemeCode = :schemecode", ObjectName);
            return ExecuteCommand(connection, command,
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input));
        }

        public static int SetObsolete(OracleConnection connection, string schemeCode, string definingParametersHash)
        {
            string command = string.Format(
                "UPDATE {0} SET IsObsolete = 1 WHERE (SchemeCode = :schemecode OR OR RootSchemeCode = :schemecode) AND DefiningParametersHash = :dphash", ObjectName);

            return ExecuteCommand(connection, command,
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash, ParameterDirection.Input));
        }
    }
}
