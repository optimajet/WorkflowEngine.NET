using System;
using System.Data;
using System.Data.SqlClient;

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

        public static WorkflowProcessScheme[] Select(SqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            var selectText = string.Format("SELECT * FROM {0} WHERE [SchemeCode] = @schemecode AND [DefiningParametersHash] = @dphash", ObjectName);

            var pSchemecode = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var pDphash = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

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

                return Select(connection, selectText, pSchemecode, pDphash, pRootSchemeId);
            }

            selectText += " AND [RootSchemeId] IS NULL";
            return Select(connection, selectText, pSchemecode, pDphash);
        }

        public static int SetObsolete(SqlConnection connection, string schemeCode)
        {
            var command = string.Format("UPDATE {0} SET [IsObsolete] = 1 WHERE [SchemeCode] = @schemecode OR [RootSchemeCode] = @schemecode", ObjectName);
            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            return ExecuteCommand(connection, command, p);
        }

        public static int SetObsolete(SqlConnection connection, string schemeCode, string definingParametersHash)
        {
            var command =
                string.Format(
                    "UPDATE {0} SET [IsObsolete] = 1 WHERE ([SchemeCode] = @schemecode OR [RootSchemeCode] = @schemecode) AND [DefiningParametersHash] = @dphash",
                    ObjectName);

            var p = new SqlParameter("schemecode", SqlDbType.NVarChar) {Value = schemeCode};

            var p2 = new SqlParameter("dphash", SqlDbType.NVarChar) {Value = definingParametersHash};

            return ExecuteCommand(connection, command, p, p2);
        }
    }
}