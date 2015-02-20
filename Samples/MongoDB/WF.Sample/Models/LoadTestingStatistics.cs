using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business;

namespace WF.Sample.Models
{
    public class LoadTestingStatisticsModel
    {
        public DateTime Date { get; set; }
        public List<LoadTestingStatisticItemModel> Items { get; set; }

        public LoadTestingStatisticsModel()
        {
            Items = new List<LoadTestingStatisticItemModel>();
        }

        public static List<LoadTestingStatisticItemModel> GetByType(List<LoadTestingStatisticsModel> stats)
        {
            var res = new List<LoadTestingStatisticItemModel>();

            foreach(var stat in stats)
            {
               foreach(var item in stat.Items)
               {
                   var r = res.FirstOrDefault(c => c.Type == item.Type);
                   if(r == null)
                   {
                       r = new LoadTestingStatisticItemModel() { 
                           Type = item.Type, 
                           MinDuration = item.MinDuration, 
                           MaxDuration = item.MaxDuration };
                       res.Add(r);
                   }
                   else
                   {
                       if(item.MinDuration < r.MinDuration)
                           r.MinDuration = item.MinDuration;

                       if(item.MaxDuration > r.MaxDuration)
                           r.MaxDuration = item.MaxDuration;
                   }

                   r.Duration += item.Duration;
                   r.Count += item.Count;
               }
            }

            return res;
        }
    }

    public class LoadTestingStatisticItemModel
    {
        public string Type { get; set; }
        public int Count { get; set; }

        public double Duration
        {
            get; set;
        }

        public double AverageDuration { get { return Duration / Count; } }

        public double? MinDuration { get; set; }
        public double? MaxDuration { get; set; }

        public void CheckDurationMinMax(double d)
        {
            if (!MinDuration.HasValue || d < MinDuration)
                MinDuration = d;

            if (!MaxDuration.HasValue || d > MaxDuration)
                MaxDuration = d;
        }
    }
}