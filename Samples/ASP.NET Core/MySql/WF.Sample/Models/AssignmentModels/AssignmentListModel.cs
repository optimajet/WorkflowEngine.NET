using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace WF.Sample.Models
{
    public class AssignmentListModel
    {
        public int Count { get; set; }

        public int PageSize { get; set; }

        public List<AssignmentItemModel> Assignments { get; set; }
        
        public Dictionary<string, AssignmentFilter> Filters { get; set; }

        public AssignmentFilterModel CustomFilter { get; set; }
        
        public List<string> Statuses { get; set; }

    }
}
