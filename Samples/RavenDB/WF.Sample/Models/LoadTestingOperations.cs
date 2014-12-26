using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business;
using WF.Sample.Business.Models;

namespace WF.Sample.Models
{
    public class LoadTestingOperationModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public double DurationMilliseconds { get; set; }
    }
}