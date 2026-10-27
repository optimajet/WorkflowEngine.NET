using System.Xml.Linq;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
#pragma warning disable 1998

namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// SQLite-based workflow provider that stores workflow schemes in files instead of the database.
    /// Inherits all persistence and runtime functionality from <see cref="SqliteProvider"/>.
    /// </summary>
    public class FileSchemePersistenceSqliteProvider : SqliteProvider
    {
        private readonly string _storePath;
        private SchemeFilePersistence _schemeFilePersistence;

        /// <summary>
        /// Initializes a new instance of <see cref="FileSchemePersistenceSqliteProvider"/>.
        /// </summary>
        /// <param name="storePath">Path to the directory where scheme files are stored.</param>
        /// <param name="connectionString">SQLite connection string.</param>
        /// <param name="schemaName">Database schema name.</param>
        /// <param name="writeToHistory">Whether to write to workflow history.</param>
        /// <param name="writeSubProcessToRoot">Whether to write subprocess data to the root process.</param>
        public FileSchemePersistenceSqliteProvider(string storePath, string connectionString, string schemaName = "public",
            bool writeToHistory = true, bool writeSubProcessToRoot = true)
            : base(connectionString, schemaName, writeToHistory, writeSubProcessToRoot)
        {
            _storePath = storePath;
        }

        /// <inheritdoc/>
        public override async Task AddSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.AddSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task AddSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            _schemeFilePersistence.AddSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task<List<string>> GetInlinedSchemeCodesAsync(string tenantId = null)
        {
            return _schemeFilePersistence.GetInlinedSchemeCodes();
        }

        /// <inheritdoc/>
        public override async Task<List<string>> GetRelatedByInliningSchemeCodesAsync(string schemeCode, string tenantId = null)
        {
            return _schemeFilePersistence.GetRelatedByInliningSchemeCodes(schemeCode);
        }

        /// <inheritdoc/>
        public override async Task<XElement> GetSchemeAsync(string code, string tenantId = null)
        {
            return _schemeFilePersistence.GetScheme(code);
        }

        /// <inheritdoc/>
        public override async Task RemoveSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.RemoveSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task RemoveSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            _schemeFilePersistence.RemoveSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task SaveSchemeAsync(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags, string tenantId = null)
        {
            _schemeFilePersistence.SaveScheme(schemaCode, canBeInlined, inlinedSchemes, scheme, tags);
        }

        /// <inheritdoc/>
        public override async Task<List<string>> SearchSchemesByTagsAsync(IEnumerable<string> tags)
        {
            return _schemeFilePersistence.SearchSchemesByTags(tags);
        }

        /// <inheritdoc/>
        public override async Task<List<string>> SearchSchemesByTagsAsync(string tenantId, IEnumerable<string> tags)
        {
            return _schemeFilePersistence.SearchSchemesByTags(tags);
        }

        /// <inheritdoc/>
        public override async Task SetSchemeTagsAsync(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.SetSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task SetSchemeTagsAsync(string schemeCode, string tenantId, IEnumerable<string> tags)
        {
            _schemeFilePersistence.SetSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task<List<string>> SearchSchemesByTagsInTenantAsync(string tenantId, params string[] tags)
        {
            return _schemeFilePersistence.SearchSchemesByTags(tags);
        }

        /// <inheritdoc/>
        public override async Task AddSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            _schemeFilePersistence.AddSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task RemoveSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            _schemeFilePersistence.RemoveSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override async Task SetSchemeTagsInTenantAsync(string schemeCode, string tenantId, params string[] tags)
        {
            _schemeFilePersistence.SetSchemeTags(schemeCode, tags);
        }

        /// <inheritdoc/>
        public override void Init(WorkflowRuntime runtime)
        {
            base.Init(runtime);
            _schemeFilePersistence = new SchemeFilePersistence(_storePath, runtime);
        }
    }
}
