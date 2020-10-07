using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
#pragma warning disable 1998

namespace OptimaJet.Workflow.PostgreSQL
{
    public class FileSchemePersistencePostgreSQLProvider : PostgreSQLProvider
    {
        private readonly string _storePath;
        private SchemeFilePersistence _schemeFilePersistence;

        public FileSchemePersistencePostgreSQLProvider(string storePath, string connectionString, string schema = "public",
            bool writeToHistory = true, bool writeSubProcessToRoot = true)
            : base(connectionString, schema, writeToHistory, writeSubProcessToRoot)
        {
            _storePath = storePath;
        }

        public override async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.AddSchemeTags(schemeCode, tags);
        }

        public override async Task<List<string>> GetInlinedSchemeCodesAsync()
        {
            return _schemeFilePersistence.GetInlinedSchemeCodes();
        }

        public override async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode)
        {
            return _schemeFilePersistence.GetRelatedByInliningSchemeCodes(schemeCode);
        }

        public override async Task<XElement> GetSchemeAsync(string code)
        {
            return _schemeFilePersistence.GetScheme(code);
        }

        public override async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.RemoveSchemeTags(schemeCode, tags);
        }

        public override async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            _schemeFilePersistence.SaveScheme(schemaCode, canBeInlined, inlinedSchemes, scheme, tags);
        }

        public override async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            return _schemeFilePersistence.SearchSchemesByTags(tags);
        }

        public override async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.SetSchemeTags(schemeCode, tags);
        }

        public override void Init(WorkflowRuntime runtime)
        {
            base.Init(runtime);
            _schemeFilePersistence = new SchemeFilePersistence(_storePath, runtime);
        }
    }
}
