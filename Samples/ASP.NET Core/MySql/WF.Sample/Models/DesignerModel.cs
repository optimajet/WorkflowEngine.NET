using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WF.Sample.Business;
using OptimaJet.Workflow.Core.Model;

namespace WF.Sample.Models
{
    public class DesignerModel
    {
        public Guid? processId { get; set; }
        public string SchemeName { get; set; }
        public string Scheme { get; set; }
    }
}
