using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF.Sample.Business;
using WF.Sample.Business.Models;

namespace WF.Sample.Models
{
    public class DocumentHistoryModel
    {
        public List<DocumentTransitionHistory> Items { get; set; }
    }
}
