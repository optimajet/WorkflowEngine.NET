using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowScheme : DbObject<WorkflowScheme>
    {
        public string Code { get; set; }
        public string Scheme { get; set; }
        public bool CanBeInlined { get; set; }
        public string InlinedSchemes { get; set; }
        public string Tags { get; set; }

        static WorkflowScheme()
        {
            DbTableName = "workflowscheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(Code), IsKey = true},
                new ColumnInfo {Name = nameof(Scheme), Type = MySqlDbType.LongText},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(InlinedSchemes)},
                new ColumnInfo {Name = nameof(Tags), Type = MySqlDbType.LongText, Size = -1}
            });
        }

        public override object GetValue(string key)
        {
            switch(key)
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
                    CanBeInlined = value.ToString() == "1";
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

        public static List<string> GetSchemeCodesByTags(MySqlConnection connection, IEnumerable<string> tags)
        {
            IEnumerable<string> tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();
            
            string query;
            var parameters = new List<MySqlParameter>();

            if (!isEmpty)
            {
                var selectBuilder = new StringBuilder($"SELECT `Code` FROM {DbTableName} WHERE ");
                var likes = new List<string>();
                foreach (string tag in tagsList)
                {
                    string paramName = $"search_{parameters.Count}";
                    string like = $"`{nameof(Tags)}` LIKE CONCAT('%',@{paramName},'%')";
                    string paramValue = $"\"{tag}\"";

                    likes.Add(like);
                    parameters.Add(new MySqlParameter(paramName, MySqlDbType.VarString) {Value = paramValue});
                }

                selectBuilder.Append(String.Join(" OR ", likes));
                query = selectBuilder.ToString();
            }
            else
            {
                query = $"SELECT `Code` FROM {DbTableName}";
            }

            return Select(connection, query, parameters.ToArray())
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public static void AddSchemeTags(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder);
        }

        public static void RemoveSchemeTags(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder);
        }

        public static void SetSchemeTags(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            UpdateSchemeTags(connection, schemeCode, schemeTags => tags.ToList(), builder);
        }

        private static void UpdateSchemeTags(MySqlConnection connection, string schemeCode,
            Func<List<string>,List<string>> getNewTags, IWorkflowBuilder builder)
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
