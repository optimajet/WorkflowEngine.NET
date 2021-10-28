using System.Collections.Generic;

namespace AbpAngularSample.Documents.Dto
{
    //WorkflowEngineSampleCode
    public class SchemeForDocumentOutput
    {
        public string Scheme { get; set; }
        public List<SchemeForDocumentCommandOutput> Commands { get; set; }
    }

    public class SchemeForDocumentCommandOutput
    {
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public string Classifier { get; set; }
    }
}