using System;
using System.Data;
#if NETCOREAPP
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowProcessScheme : DbObject<WorkflowProcessScheme>
    {
        static WorkflowProcessScheme()
        {
            DbTableName = "WorkflowProcessScheme";
        }

        public WorkflowProcessScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "DefiningParameters"},
                new ColumnInfo {Name = "DefiningParametersHash"},
                new ColumnInfo {Name = "IsObsolete", Type = SqlDbType.Bit},
                new ColumnInfo {Name = "SchemeCode"},
                new ColumnInfo {Name = "Scheme", Type = SqlDbType.NVarChar, Size = -1},
                new ColumnInfo {Name = "RootSchemeId", Type = SqlDbType.UniqueIdentifier},
                new ColumnInfo {Name = "RootSchemeCode"},
                new ColumnInfo {Name = "AllowedActivities"},
                new ColumnInfo {Name = "StartingTransition"}
            });
        }

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

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id;
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
                    return RootSchemeId;
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
                    Id = (Guid) value;
                    break;
                case "DefiningParameters":
                    DefiningParameters = value as string;
                    break;
                case "DefiningParametersHash":
                    DefiningParametersHash = value as string;
                    break;
                case "IsObsolete":
                    IsObsolete = (bool) value;
                    break;
                case "SchemeCode":
                    SchemeCode = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                case "RootSchemeId":
                    RootSchemeId = value as Guid?;
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

        public static async Task<WorkflowProcessScheme[]> SelectAsync(SqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE [SchemeCode] = @schemecode AND [DefiningParametersHash] = @dphash", ObjectName);

            var pSchemeCode = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var pHash = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += " AND [IsObsolete] = 1";
                }
                else
                {
                    selectText += " AND [IsObsolete] = 0";
                }
            }

            if (rootSchemeId.HasValue)
            {
                selectText += " AND [RootSchemeId] = @drootschemeid";
                var pRootSchemeId = new SqlParameter("drootschemeid", SqlDbType.UniqueIdentifier)
                {
                    Value = rootSchemeId.Value
                };

                return await SelectAsync(connection, selectText, pSchemeCode, pHash, pRootSchemeId).ConfigureAwait(false);
            }

            selectText += " AND [RootSchemeId] IS NULL";
            return await SelectAsync(connection, selectText, pSchemeCode, pHash).ConfigureAwait(false);
        }

        public static async Task<int> SetObsoleteAsync(SqlConnection connection, string schemeCode)
        {
            string command = $"UPDATE {ObjectName} SET [IsObsolete] = 1 WHERE [SchemeCode] = @schemecode OR [RootSchemeCode] = @schemecode";
            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            return await ExecuteCommandNonQueryAsync(connection, command, p).ConfigureAwait(false);
        }

        public static async Task<int> SetObsoleteAsync(SqlConnection connection, string schemeCode, string definingParametersHash)
        {
            string command = $"UPDATE {ObjectName} SET [IsObsolete] = 1 WHERE ([SchemeCode] = @schemecode OR [RootSchemeCode] = @schemecode) AND [DefiningParametersHash] = @dphash";

            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var p2 = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

            return await ExecuteCommandNonQueryAsync(connection, command, p, p2).ConfigureAwait(false);
        }
    }
}
