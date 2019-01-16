using System;
using Npgsql;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
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
            DBColumns.AddRange(new[]{
                new ColumnInfo {Name="Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name="DefiningParameters"},
                new ColumnInfo {Name="DefiningParametersHash" },
                new ColumnInfo {Name="IsObsolete", Type = NpgsqlDbType.Boolean },
                new ColumnInfo {Name="SchemeCode" },
                new ColumnInfo {Name="Scheme" },
                new ColumnInfo {Name = "RootSchemeId",Type = NpgsqlDbType.Uuid},
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
                    Id = (Guid)value;
                    break;
                case "DefiningParameters":
                    DefiningParameters = value as string;
                    break;
                case "DefiningParametersHash":
                    DefiningParametersHash = value as string;
                    break;
                case "IsObsolete":
                    IsObsolete = (bool)value;
                    break;
                case "SchemeCode":
                    SchemeCode = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                case "RootSchemeId":
                    if (value is Guid)
                    {
                        RootSchemeId = (Guid) value;
                    }
                    else
                    {
                        RootSchemeId = null;
                    }
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

        public static WorkflowProcessScheme[] Select(NpgsqlConnection connection, string schemeCode, string definingParametersHash, bool? isObsolete, Guid? rootSchemeId)
        {
            string selectText = string.Format("SELECT * FROM {0} WHERE \"SchemeCode\" = @schemecode AND \"DefiningParametersHash\" = @dphash", ObjectName);

            if (isObsolete.HasValue)
            {
                if (isObsolete.Value)
                {
                    selectText += " AND \"IsObsolete\" = TRUE";
                }
                else
                {
                    selectText += " AND \"IsObsolete\" = FALSE";
                }
            }

            var pSchemecode = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};

            var pDphash = new NpgsqlParameter("dphash", NpgsqlDbType.Varchar) {Value = definingParametersHash};

            if (rootSchemeId.HasValue)
            {
                selectText += " AND \"RootSchemeId\" = @rootschemeid";
                var pRootSchemeId = new NpgsqlParameter("rootschemeid", NpgsqlDbType.Uuid) {Value = rootSchemeId.Value};

                return Select(connection, selectText, pSchemecode, pDphash, pRootSchemeId);
            }
            else
            {
                selectText += " AND \"RootSchemeId\" IS NULL";
                return Select(connection, selectText, pSchemecode, pDphash);
            }
        }

        public static int SetObsolete(NpgsqlConnection connection, string schemeCode)
        {
            string command = string.Format("UPDATE {0} SET \"IsObsolete\" = TRUE WHERE \"SchemeCode\" = @schemecode OR \"RootSchemeCode\" = @schemecode", ObjectName);
            var p = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};

            return ExecuteCommand(connection, command, p);
        }

        public static int SetObsolete(NpgsqlConnection connection, string schemeCode, string definingParametersHash)
        {
            string command =
                string.Format(
                    "UPDATE {0} SET \"IsObsolete\" = TRUE WHERE (\"SchemeCode\" = @schemecode OR \"RootSchemeCode\" = @schemecode) AND \"DefiningParametersHash\" = @dphash",
                    ObjectName);

            var p = new NpgsqlParameter("schemecode", NpgsqlDbType.Varchar) {Value = schemeCode};
            var p2 = new NpgsqlParameter("dphash", NpgsqlDbType.Varchar) {Value = definingParametersHash};

            return ExecuteCommand(connection, command, p, p2);
        }
    }
}
