using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace AngularBPWorkflow.Documents.Dto
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
