using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
        
        public List<string> GetInlinedSchemes()
        {
            return JsonConvert.DeserializeObject<List<string>>(InlinedSchemes);
        }
        
        public static async Task<List<string>> GetInlinedSchemeCodesAsync(MySqlConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE `CanBeInlined` = 1";
            WorkflowScheme[] schemes = await SelectAsync(connection, selectText).ConfigureAwait(false);
            return schemes.Select(sch => sch.Code).ToList();
        }
        
        public static async Task<List<string>> GetRelatedSchemeCodesAsync(MySqlConnection connection, string schemeCode)
        {
            string selectText =  $"SELECT * FROM {DbTableName} WHERE `{nameof(InlinedSchemes)}` LIKE CONCAT('%',@search,'%')";
            var p = new MySqlParameter("search", MySqlDbType.VarString) {Value = $"\"{schemeCode}\""};
            return (await SelectAsync(connection, selectText, p).ConfigureAwait(false)).Select(sch=>sch.Code).Distinct().ToList();
        }

        public static async Task<List<string>> GetSchemeCodesByTagsAsync(MySqlConnection connection, IEnumerable<string> tags)
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

            return (await SelectAsync(connection, query, parameters.ToArray()).ConfigureAwait(false))
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public static async Task AddSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder).ConfigureAwait(false);
        }

        public static async Task RemoveSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder).ConfigureAwait(false);
        }

        public static async Task SetSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => tags.ToList(), builder).ConfigureAwait(false);
        }

        private static async Task UpdateSchemeTagsAsync(MySqlConnection connection, string schemeCode,
            Func<List<string>,List<string>> getNewTags, IWorkflowBuilder builder)
        {
            WorkflowScheme scheme = await SelectByKeyAsync(connection, schemeCode).ConfigureAwait(false);

            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowScheme);
            }

            List<string> newTags = getNewTags.Invoke(TagHelper.FromTagStringForDatabase(scheme.Tags));
            scheme.Tags = TagHelper.ToTagStringForDatabase(newTags);
            scheme.Scheme = builder.ReplaceTagsInScheme(scheme.Scheme, newTags);

            await scheme.UpdateAsync(connection).ConfigureAwait(false);
        }
    }
}
