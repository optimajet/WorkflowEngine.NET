using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WF.Sample.MongoDb.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public int? Number { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }
        
        public Guid AuthorId { get; set; }
        
        public string AuthorName { get; set; }

        public Guid? ManagerId { get; set; }

        public string ManagerName { get; set; }

        public decimal Sum { get; set; }

        public string State { get; set; }
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
}