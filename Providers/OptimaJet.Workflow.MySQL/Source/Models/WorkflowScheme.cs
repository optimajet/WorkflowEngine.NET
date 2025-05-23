using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowScheme : DbObject<SchemeEntity>
    {
        public WorkflowScheme(int commandTimeout) : base("workflowscheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SchemeEntity.Code), IsKey = true},
                new ColumnInfo {Name = nameof(SchemeEntity.Scheme), Type = MySqlDbType.LongText},
                new ColumnInfo {Name = nameof(SchemeEntity.CanBeInlined), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(SchemeEntity.InlinedSchemes)}, 
                new ColumnInfo {Name = nameof(SchemeEntity.Tags), Type = MySqlDbType.LongText, Size = -1}
            });
        }

        public async Task<SchemeEntity[]> SelectAllWorkflowSchemesWithPagingAsync(MySqlConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters, Paging paging)
        {
            return await SelectAllWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);
        }
        
        public async Task<List<string>> GetInlinedSchemeCodesAsync(MySqlConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE `{nameof(SchemeEntity.CanBeInlined)}` = 1";
            
            SchemeEntity[] schemes = await SelectAsync(connection, selectText).ConfigureAwait(false);
            return schemes.Select(sch => sch.Code).ToList();
        }
        
        public async Task<List<string>> GetRelatedSchemeCodesAsync(MySqlConnection connection, string schemeCode)
        {
            string selectText =  $"SELECT * FROM {DbTableName} " + 
                                 $"WHERE `{nameof(SchemeEntity.InlinedSchemes)}` LIKE CONCAT('%',@search,'%')";
            
            var p = new MySqlParameter("search", MySqlDbType.VarString) {Value = $"\"{schemeCode}\""};
            return (await SelectAsync(connection, selectText, p).ConfigureAwait(false)).Select(sch=>sch.Code).Distinct().ToList();
        }

        public async Task<List<string>> GetSchemeCodesByTagsAsync(MySqlConnection connection, IEnumerable<string> tags)
        {
            IEnumerable<string> tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();
            
            string query;
            var parameters = new List<MySqlParameter>();

            if (!isEmpty)
            {
                var selectBuilder = new StringBuilder($"SELECT `{nameof(SchemeEntity.Code)}` FROM {DbTableName} WHERE ");
                var likes = new List<string>();
                foreach (string tag in tagsList)
                {
                    string paramName = $"search_{parameters.Count}";
                    string like = $"`{nameof(SchemeEntity.Tags)}` LIKE CONCAT('%',@{paramName},'%')";
                    string paramValue = $"\"{tag}\"";

                    likes.Add(like);
                    parameters.Add(new MySqlParameter(paramName, MySqlDbType.VarString) {Value = paramValue});
                }

                selectBuilder.Append(String.Join(" OR ", likes));
                query = selectBuilder.ToString();
            }
            else
            {
                query = $"SELECT `{nameof(SchemeEntity.Code)}` FROM {DbTableName}";
            }

            return (await SelectAsync(connection, query, parameters.ToArray()).ConfigureAwait(false))
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public async Task AddSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder).ConfigureAwait(false);
        }

        public async Task RemoveSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder).ConfigureAwait(false);
        }

        public async Task SetSchemeTagsAsync(MySqlConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => tags.ToList(), builder).ConfigureAwait(false);
        }

        private async Task UpdateSchemeTagsAsync(MySqlConnection connection, string schemeCode,
            Func<List<string>,List<string>> getNewTags, IWorkflowBuilder builder)
        {
            SchemeEntity scheme = await SelectByKeyAsync(connection, schemeCode).ConfigureAwait(false);

            if (scheme == null)
            {
                throw SchemeNotFoundException.Create(schemeCode, SchemeLocation.WorkflowScheme);
            }

            List<string> newTags = getNewTags.Invoke(TagHelper.FromTagStringForDatabase(scheme.Tags));
            scheme.Tags = TagHelper.ToTagStringForDatabase(newTags);
            scheme.Scheme = builder.ReplaceTagsInScheme(scheme.Scheme, newTags);

            await UpdateAsync(connection, scheme).ConfigureAwait(false);
        }
    }
}
