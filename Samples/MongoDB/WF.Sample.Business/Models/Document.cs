using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WF.Sample.Business.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public long? Number { get; set; }

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

        public Document ()
        {
            TransitionHistories = new List<DocumentTransitionHistory>();
        }

        public string StateNameToSet { get; set; }

        private List<DocumentTransitionHistory> _transitionHistories = null;
        public List<DocumentTransitionHistory> TransitionHistories
        {
            get
            {
                if (_transitionHistories == null)
                    _transitionHistories = new List<DocumentTransitionHistory>();
                return _transitionHistories;
            }
            set { _transitionHistories = value; }
        }
    }

    public class DocumentCommandModel
    {
        public string key { get; set; }
        public string value { get; set; }
        public OptimaJet.Workflow.Core.Model.TransitionClassifier Classifier { get; set; }
    }
}