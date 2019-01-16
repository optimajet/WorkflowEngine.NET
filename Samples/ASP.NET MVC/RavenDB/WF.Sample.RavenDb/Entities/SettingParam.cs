using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Sample.RavenDb.Entities
{
    public class SettingParam<T>
    {
        public string Id { get; set; }
        public T Value { get; set; }
    }
}
