﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.MsSql
{
    [Table("EmployeeRole")]
    public class EmployeeRole
    {
        public Guid EmployeeId { get; set; }
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual Employee Employee { get; set; }

    }
}
