using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business;

namespace WF.Sample.Models
{
    public class DocumentModel
    {
        public Guid Id { get; set; }
        public int? Number { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }
        
        public Guid AuthorId { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Controller")]
        public Guid? EmloyeeControlerId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Controller")]
        public string EmloyeeControlerName { get; set; }

        [Display(Name = "Sum")]
        public decimal Sum { get; set; }

        [Display(Name = "State")]
        public string StateName { get; set; }

        public DocumentCommandModel[] Commands { get; set; }

        public Dictionary<string, string> AvailiableStates { get; set; }

        public DocumentModel ()
        {
            Commands = new DocumentCommandModel[0];
            AvailiableStates = new Dictionary<string, string>{};
            HistoryModel = new DocumentHistoryModel();
        }

        public string StateNameToSet { get; set; }

        public DocumentHistoryModel HistoryModel { get; set; }
    }

    public class DocumentCommandModel
    {
        public string key { get; set; }
        public string value { get; set; }
        public OptimaJet.Workflow.Core.Model.TransitionClassifier Classifier { get; set; }
    }
}