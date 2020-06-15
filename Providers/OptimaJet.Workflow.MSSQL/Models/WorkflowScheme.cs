using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

// ReSharper disable once CheckNamespace

namespace OptimaJet.Workflow.DbPersistence
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        static WorkflowScheme()
        {
            DbTableName = "WorkflowScheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(Code), IsKey = true},
                new ColumnInfo {Name = nameof(Scheme), Size = -1},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = SqlDbType.Bit},
                new ColumnInfo {Name = nameof(InlinedSchemes)}, new ColumnInfo {Name = nameof(Tags), Size = -1}
            });
        }

        public string Code { get; set; }
        public string Scheme { get; set; }
        public bool CanBeInlined { get; set; }
        public string InlinedSchemes { get; set; }
        public string Tags { get; set; }


        public override object GetValue(string key)
        {
            switch (key)
            {
                case nameof(Code):
                    return Code;
                case nameof(Scheme):
                    return Scheme;
                case nameof(CanBeInlined):
                    return CanBeInlined;
                case nameof(InlinedSchemes):
                    return InlinedSchemes;
                case nameof(Tags):
                    return Tags;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case nameof(Code):
                    Code = value as string;
                    break;
                case nameof(Scheme):
                    Scheme = value as string;
                    break;
                case nameof(CanBeInlined):
                    CanBeInlined = (bool)value;
                    break;
                case nameof(InlinedSchemes):
                    InlinedSchemes = value as string;
                    break;
                case nameof(Tags):
                    Tags = value as string;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static List<string> GetInlinedSchemeCodes(SqlConnection connection)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE [CanBeInlined] = 1";
            var schemes = Select(connection, selectText);
            return schemes.Select(sch => sch.Code).ToList();
        }

        public static List<string> GetRelatedSchemeCodes(SqlConnection connection, string schemeCode)
        {
            var selectText = $"SELECT * FROM {ObjectName} WHERE [{nameof(InlinedSchemes)}] LIKE '%' + @search + '%'";
            var p = new SqlParameter("search", SqlDbType.NVarChar) {Value = $"\"{schemeCode}\""};
            return Select(connection, selectText, p).Select(sch => sch.Code).Distinct().ToList();
        }

        public static List<string> GetSchemeCodesByTags(SqlConnection connection, IEnumerable<string> tags)
        {
            IEnumerable<string> tagsList = tags?.ToList();

            bool isEmpty = tagsList == null || !tagsList.Any();

            string query;
            var parameters = new List<SqlParameter>();
            
            if (!isEmpty)
            {
                var selectBuilder = new StringBuilder($"SELECT Code FROM {ObjectName} WHERE ");
               
                var likes = new List<string>();
                foreach (string tag in tagsList)
                {
                    string paramName = $"search_{parameters.Count}";
                    string like = $"[{nameof(Tags)}] LIKE '%' + @{paramName} + '%'";
                    string paramValue = $"\"{tag}\"";

                    likes.Add(like);
                    parameters.Add(new SqlParameter(paramName, SqlDbType.NVarChar) {Value = paramValue});
                }

                selectBuilder.Append(String.Join(" OR ", likes));

                query = selectBuilder.ToString();

            }
            else
            {
                query = $"SELECT Code FROM {ObjectName}";
            }


            return Select(connection, query, parameters.ToArray())
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public static void AddSchemeTags(SqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder);
        }

        public static void RemoveSchemeTags(SqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder);
        }

        public static void SetSchemeTags(SqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => tags.ToList(), builder);
        }

        private static void UpdateSchemeTags(SqlConnection connection, string schemeCode,
            Func<List<string>, List<string>> getNewTags, IWorkflowBuilder builder)
        {
            WorkflowScheme scheme = SelectByKey(connection, schemeCode);

            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowScheme);
            }

            List<string> newTags = getNewTags.Invoke(TagHelper.FromTagStringForDatabase(scheme.Tags));
            scheme.Tags = TagHelper.ToTagStringForDatabase(newTags);
            scheme.Scheme = builder.ReplaceTagsInScheme(scheme.Scheme, newTags);

            scheme.Update(connection);
        }
    }
}
