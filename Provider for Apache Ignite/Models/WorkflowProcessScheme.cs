using System;
using System.Data;
using System.Data.SqlClient;
using Apache.Ignite.Core.Cache.Configuration;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.Ignite
{
    public class WorkflowProcessScheme : DbObject<WorkflowProcessScheme>
    {
        static WorkflowProcessScheme()
        {
            DbColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = typeof(Guid)},
                new ColumnInfo {Name = "DefiningParameters"},
                new ColumnInfo {Name = "DefiningParametersHash"},
                new ColumnInfo {Name = "IsObsolete", Type = typeof(bool)},
                new ColumnInfo {Name = "SchemeCode"},
                new ColumnInfo {Name = "Scheme"},
                new ColumnInfo {Name = "RootSchemeId"},
                new ColumnInfo {Name = "RootSchemeCode"},
                new ColumnInfo {Name = "AllowedActivities"},
                new ColumnInfo {Name = "StartingTransition"}
            });
        }

        public string DefiningParameters { get; set; }

        [QuerySqlField]
        public string DefiningParametersHash { get; set; }

        public Guid Id { get; set; }

        [QuerySqlField]
        public bool IsObsolete { get; set; }

        [QuerySqlField]
        public string SchemeCode { get; set; }

        public string Scheme { get; set; }

        [QuerySqlField]
        public string RootSchemeId { get; set; }

        [QuerySqlField]
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
                    RootSchemeId = value as string;
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
    }
}