using System;
using System.Threading.Tasks;
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

        public static async Task<WorkflowProcessScheme[]> SelectAsync(MySqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId )
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `SchemeCode` = @schemecode AND `DefiningParametersHash` = @dphash";

            var pSchemeCode = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            var pHash = new MySqlParameter("dphash", MySqlDbType.VarString) {Value = definingParametersHash};

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

                return await SelectAsync(connection, selectText, pSchemeCode, pHash, pRootSchemeId).ConfigureAwait(false);
            }

            selectText += " AND `ROOTSCHEMEID` IS NULL";
            return await SelectAsync(connection, selectText, pSchemeCode, pHash).ConfigureAwait(false);
        }

        public static async Task<int> SetObsoleteAsync(MySqlConnection connection, string schemeCode)
        {
            string command = $"UPDATE {DbTableName} SET `IsObsolete` = 1 WHERE `SchemeCode` = @schemecode OR `RootSchemeCode` = @schemecode";
            var p = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            return await ExecuteCommandNonQueryAsync(connection, command, p).ConfigureAwait(false);
        }

        public static async Task<int> SetObsoleteAsync(MySqlConnection connection, string schemeCode, string definingParametersHash)
        {
            string command =
                $"UPDATE {DbTableName} SET `IsObsolete` = 1 WHERE (`SchemeCode` = @schemecode OR `RootSchemeCode` = @schemecode) AND `DefiningParametersHash` = @dphash";

            var p = new MySqlParameter("schemecode", MySqlDbType.VarString) {Value = schemeCode};

            var p2 = new MySqlParameter("dphash", MySqlDbType.VarString) {Value = definingParametersHash};

            return await ExecuteCommandNonQueryAsync(connection, command, p, p2).ConfigureAwait(false);
        }
    }
}
