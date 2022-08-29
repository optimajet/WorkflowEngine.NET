using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowScheme : DbObject<SchemeEntity>
    {
        public WorkflowScheme(string schemaName, int commandTimeout) : base(schemaName, "WorkflowScheme", commandTimeout)
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(SchemeEntity.Code), IsKey = true},
                new ColumnInfo {Name = nameof(SchemeEntity.Scheme), Type = OracleDbType.Clob},
                new ColumnInfo {Name = nameof(SchemeEntity.CanBeInlined), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(SchemeEntity.InlinedSchemes)}, 
                new ColumnInfo {Name = nameof(SchemeEntity.Tags), Type = OracleDbType.NVarchar2}
            });
        }

        public async Task<SchemeEntity[]> SelectAllWorkflowSchemesWithPagingAsync(OracleConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters, Paging paging)
        {
            return await SelectAllWithPagingAsync(connection, orderParameters, paging).ConfigureAwait(false);
        }
        
        public async Task<List<string>> GetInlinedSchemeCodesAsync(OracleConnection connection)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE {nameof(SchemeEntity.CanBeInlined).ToUpper()} = 1";
            
            var schemes = (await SelectAsync(connection, selectText).ConfigureAwait(false)).ToList();
            return schemes.Select(sch => sch.Code).ToList();
        }

        public async Task<List<string>> GetRelatedSchemeCodesAsync(OracleConnection connection, string schemeCode)
        {
            string selectText = $"SELECT * FROM {DbTableName} " + 
                                $"WHERE {nameof(SchemeEntity.InlinedSchemes).ToUpper()} LIKE '%' || :search || '%'";
            
            var p = new OracleParameter("search", OracleDbType.NVarchar2, $"\"{schemeCode}\"", ParameterDirection.Input);
            return (await SelectAsync(connection, selectText, p).ConfigureAwait(false)).Select(sch => sch.Code).ToList();
        }

        public async Task<List<string>> GetSchemeCodesByTagsAsync(OracleConnection connection, IEnumerable<string> tags)
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
                    string like = $"{nameof(SchemeEntity.Tags).ToUpper()} LIKE '%' || :{paramName} || '%'";
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

        public async Task AddSchemeTagsAsync(OracleConnection connection, string schemeCode, IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Concat(tags).ToList(), builder).ConfigureAwait(false);
        }

        public async Task RemoveSchemeTagsAsync(OracleConnection connection, string schemeCode,
            IEnumerable<string> tags, IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => schemeTags.Where(t => !tags.Contains(t)).ToList(),
                builder).ConfigureAwait(false);
        }

        public async Task SetSchemeTagsAsync(OracleConnection connection,
            string schemeCode,
            IEnumerable<string> tags,
            IWorkflowBuilder builder)
        {
            await UpdateSchemeTagsAsync(connection, schemeCode, schemeTags => tags.ToList(), builder).ConfigureAwait(false);
        }

        private async Task UpdateSchemeTagsAsync(OracleConnection connection, string schemeCode,
            Func<List<string>, List<string>> getNewTags, IWorkflowBuilder builder)
        {
            var scheme = await SelectByKeyAsync(connection, schemeCode).ConfigureAwait(false);

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
