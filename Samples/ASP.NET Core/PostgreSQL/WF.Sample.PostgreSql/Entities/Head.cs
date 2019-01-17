using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.PostgreSql
{
    [Table("vHeads")]
    public class Head
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid HeadId { get; set; }
        public string HeadName { get; set; }
    }
}
