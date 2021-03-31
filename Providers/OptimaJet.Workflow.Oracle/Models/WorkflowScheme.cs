using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
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
            DbTableName = "WorkflowScheme";
        }

        public WorkflowScheme()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(Code), IsKey = true}, new ColumnInfo {Name = nameof(Scheme), Type = OracleDbType.Clob},
                new ColumnInfo {Name = nameof(CanBeInlined), Type = OracleDbType.Byte}, new ColumnInfo {Name = nameof(InlinedSchemes)},
                new ColumnInfo {Name = nameof(Tags), Type = OracleDbType.Clob}
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case nameof(Code):
                    return Code;
                case nameof(Scheme):
                    return Scheme;
                case nameof(CanBeInlined):
                    return CanBeInlined ? "1" : "0";
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
                    CanBeInlined = (string)value == "1";
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
        
        public static async Task<List<string>> GetInlinedSchemeCodesAsync(OracleConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE {nameof(CanBeInlined).ToUpper()} = 1";
            var schemes = (await SelectAsync(connection, selectText).ConfigureAwait(false)).ToList();
            return schemes.Select(sch => sch.Code).ToList();
        }

        public static async Task<List<string>> GetRelatedSchemeCodesAsync(OracleConnection connection, string schemeCode)
        {
            string selectText = $"SELECT * FROM {DbTableName} WHERE {nameof(InlinedSchemes).ToUpper()} LIKE '%' || :search || '%'";
            var p = new OracleParameter("search", OracleDbType.NVarchar2, $"\"{schemeCode}\"", ParameterDirection.Input);
            return (await SelectAsync(connection, selectText, p).ConfigureAwait(false)).Select(sch => sch.Code).ToList();
        }

        public static async Task<List<string>> GetSchemeCodesByTagsAsync(OracleConnection connection, IEnumerable<string> tags)
        {
            IEnumerable<string> tagsList = tags?.ToList();

            bool isEmpty = tagsList == null || !tagsList.Any();

            string query;
            var parameters = new List<OracleParameter>();

            if (!isEmpty)
            {
                var selectBuilder = new StringBuilder($"SELECT * FROM {DbTableName} WHERE ");
                var likes = new List<string>();
                foreach (string tag in tagsList)
                {
                    string paramName = $"search_{parameters.Count}";
                    string like = $"{nameof(Tags).ToUpper()} LIKE '%' || :{paramName} || '%'";
                    string paramValue = $"\"{tag}\"";

                    likes.Add(like);
                    parameters.Add(new OracleParameter(paramName, OracleDbType.NVarchar2, paramValue, ParameterDirection.Input));
                }

                selectBuilder.Append(String.Join(" OR ", likes));
                query = selectBuilder.ToString();
            }
            else
            {
                query = $"SELECT * FROM {DbTableName}";
            }

            return (await SelectAsync(connection, query, parameters.ToArray()).ConfigureAwait(false))
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public static async Task AddSchemeTagsAsync(OracleConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder).ConfigureAwait(false);
        }

        public static async Task RemoveSchemeTagsAsync(OracleConnection connection, string schemeCode,
            IEnumerable<string> tags, IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder).ConfigureAwait(false);
        }

        public static async Task SetSchemeTagsAsync(OracleConnection connection,
            string schemeCode,
            IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => tags.ToList(), builder).ConfigureAwait(false);
        }

        private static async Task UpdateSchemeTagsAsync(OracleConnection connection, string schemeCode,
            Func<List<string>, List<string>> getNewTags, IWorkflowBuilder builder)
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
            await CommitAsync(connection).ConfigureAwait(false);
        }
    }
}
