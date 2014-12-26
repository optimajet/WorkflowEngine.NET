using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Sample.Business.Models
{
    public class SettingParam<T>
    {
        public string Id { get; set; }
        public T Value { get; set; }
    }
}
