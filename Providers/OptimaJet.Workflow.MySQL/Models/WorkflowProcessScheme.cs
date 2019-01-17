using System;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
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
            DbTableName = "workflowprocessscheme";
        }

        public WorkflowProcessScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = "DefiningParameters"},
                new ColumnInfo {Name = "DefiningParametersHash"},
                new ColumnInfo {Name = "IsObsolete", Type = MySqlDbType.Bit},
                new ColumnInfo {Name = "SchemeCode"},
                new ColumnInfo {Name = "Scheme"},
                new ColumnInfo {Name = "RootSchemeId", Type = MySqlDbType.Binary},
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
                    return IsObsolete;
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
                    Id = new Guid((byte[])value);
                    break;
                case "DefiningParameters":
                    DefiningParameters = value as string;
                    break;
                case "DefiningParametersHash":
                    DefiningParametersHash = value as string;
                    break;
                case "IsObsolete":
                    IsObsolete = value.ToString() == "1";
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

        public static WorkflowProcessScheme[] Select(MySqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId )
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE `SchemeCode` = @schemecode AND `DefiningParametersHash` = @dphash", DbTableName);

            var pSchemecode = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            var pDphash = new MySqlParameter("dphash", MySqlDbType.VarString) {Value = definingParametersHash};

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += " AND `ISOBSOLETE` = 1";
                }
                else 
                {
                    selectText += " AND `ISOBSOLETE` = 0";
                }
            }

            if (rootSchemeId.HasValue)
            {
                selectText += " AND `ROOTSCHEMEID` = @drootschemeid";
                var pRootSchemeId = new MySqlParameter("drootschemeid", MySqlDbType.Binary) {Value = rootSchemeId.Value.ToByteArray()};

                return Select(connection, selectText, pSchemecode, pDphash, pRootSchemeId);
            }
            else
            {
                selectText += " AND `ROOTSCHEMEID` IS NULL";
                return Select(connection, selectText, pSchemecode, pDphash);
            }
        }

        public static int SetObsolete(MySqlConnection connection, string schemeCode)
        {
            var command = string.Format("UPDATE {0} SET `IsObsolete` = 1 WHERE `SchemeCode` = @schemecode OR `RootSchemeCode` = @schemecode", DbTableName);
            var p = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            return ExecuteCommand(connection, command, p);
        }

        public static int SetObsolete(MySqlConnection connection, string schemeCode, string definingParametersHash)
        {
            var command =
                string.Format("UPDATE {0} SET `IsObsolete` = 1 WHERE (`SchemeCode` = @schemecode OR `RootSchemeCode` = @schemecode) AND `DefiningParametersHash` = @dphash",
                    DbTableName);

            var p = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            var p2 = new MySqlParameter("dphash", MySqlDbType.VarString) {Value = definingParametersHash};

            return ExecuteCommand(connection, command, p, p2);
        }
    }
}
