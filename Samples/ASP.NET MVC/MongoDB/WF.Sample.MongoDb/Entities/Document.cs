using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WF.Sample.MongoDb.Entities
{
    public class Document
    {
        public Document()
        {
            State = "VacationRequestCreated";
            StateName = "Vacation request created";
        }
        
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
        
    }
}
