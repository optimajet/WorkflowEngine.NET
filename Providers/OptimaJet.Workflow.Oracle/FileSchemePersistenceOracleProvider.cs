using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Oracle
{
    public class FileSchemePersistenceOracleProvider : OracleProvider
    {
        private readonly string _storePath;
        private SchemeFilePersistence _schemeFilePersistence;

        public FileSchemePersistenceOracleProvider(string storePath, string connectionString, string schema = null,
            bool writeToHistory = true, bool writeSubProcessToRoot = true)
            : base(connectionString, schema, writeToHistory, writeSubProcessToRoot)
        {
            _storePath = storePath;
        }

        public override void AddSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.AddSchemeTags(schemeCode, tags);
        }

        public override List<string> GetInlinedSchemeCodes()
        {
            return _schemeFilePersistence.GetInlinedSchemeCodes();
        }

        public override List<string> GetRelatedByInliningSchemeCodes(string schemeCode)
        {
            return _schemeFilePersistence.GetRelatedByInliningSchemeCodes(schemeCode);
        }

        public override XElement GetScheme(string code)
        {
            return _schemeFilePersistence.GetScheme(code);
        }

        public override void RemoveSchemeTags(string schemeCode, IEnumerable<string> tags)
        {
            _schemeFilePersistence.RemoveSchemeTags(schemeCode, tags);
        }

        public override void SaveScheme(string schemaCode, bool canBeInlined, List<string> inlinedSchemes, string scheme, List<string> tags)
        {
            _schemeFilePersistence.SaveScheme(schemaCode, canBeInlined, inlinedSchemes, scheme, tags);
        }

        public override List<string> SearchSchemesByTags(IEnumerable<string> tags)
        {
            return _schemeFilePersistence.SearchSchemesByTags(tags);
        }

        public override void SetSchemeTags(string schemeCode, IEnumerable<string> tags)
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
