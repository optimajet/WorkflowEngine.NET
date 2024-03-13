using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowScheme : DbObject<SchemeEntity>
    {
        public WorkflowScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SchemeEntity.Code), IsKey = true},
                new ColumnInfo {Name = nameof(SchemeEntity.Scheme)},
                new ColumnInfo {Name = nameof(SchemeEntity.CanBeInlined), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(SchemeEntity.InlinedSchemes)}, 
                new ColumnInfo {Name = nameof(SchemeEntity.Tags)}
            });
        }

        public async Task<SchemeEntity[]> SelectAllWorkflowSchemesWithPagingAsync(SqliteConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters, Paging paging)
        {
            return await SelectAllWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);
        }
        
        public async Task<List<string>> GetInlinedSchemeCodesAsync(SqliteConnection connection)
        {
            string selectText = $"SELECT * FROM {ObjectName} " + 
                                $"WHERE {nameof(SchemeEntity.CanBeInlined)} = TRUE";
            
            SchemeEntity[] schemes = await SelectAsync(connection, selectText).ConfigureAwait(false);
            return schemes.Select(sch => sch.Code).ToList();
        }
        
        public async Task<List<string>> GetRelatedSchemeCodesAsync(SqliteConnection connection, string schemeCode)
        {
            string selectText =  $"SELECT * FROM {ObjectName} " + 
                                 $"WHERE {nameof(SchemeEntity.InlinedSchemes)} LIKE '%' || @search || '%'";
            
            var p = new SqliteParameter("search", DbType.String) {Value = $"{schemeCode}"};
            return (await SelectAsync(connection, selectText, p).ConfigureAwait(false)).Select(sch=>sch.Code).Distinct().ToList();
        }

        public async Task<List<string>> GetSchemeCodesByTagsAsync(SqliteConnection connection, IEnumerable<string> tags)
        {
            IEnumerable<string> tagsList = tags?.ToList();
            bool isEmpty = tagsList == null || !tagsList.Any();

            string query;
            var parameters = new List<SqliteParameter>();

            if (!isEmpty)
            {
                var selectBuilder = new StringBuilder($"SELECT {nameof(SchemeEntity.Code)} FROM {ObjectName} WHERE ");
                var likes = new List<string>();
                foreach (string tag in tagsList)
                {
                    string paramName = $"search_{parameters.Count}";
                    string like = $"{nameof(SchemeEntity.Tags)} LIKE '%' || @{paramName} || '%'";
                    string paramValue = $"{tag}";

                    likes.Add(like);
                    parameters.Add(new SqliteParameter(paramName, DbType.String) {Value = paramValue});
                }

                selectBuilder.Append(String.Join(" OR ", likes));
                query = selectBuilder.ToString();
            }
            else
            {
                query = $"SELECT {nameof(SchemeEntity.Code)} FROM {ObjectName}";
            }

            return (await SelectAsync(connection, query, parameters.ToArray()).ConfigureAwait(false))
                .Select(sch => sch.Code)
                .Distinct()
                .ToList();
        }

        public async Task AddSchemeTagsAsync(SqliteConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder).ConfigureAwait(false);
        }

        public  async Task RemoveSchemeTagsAsync(SqliteConnection connection, string schemeCode,
            IEnumerable<string> tags, IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder).ConfigureAwait(false);
        }

        public async Task SetSchemeTagsAsync(SqliteConnection connection,
            string schemeCode,
            IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => tags.ToList(), builder).ConfigureAwait(false);
        }

        private async Task UpdateSchemeTagsAsync(SqliteConnection connection, string schemeCode,
            Func<List<string>, List<string>> getNewTags, IWorkflowBuilder builder)
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
