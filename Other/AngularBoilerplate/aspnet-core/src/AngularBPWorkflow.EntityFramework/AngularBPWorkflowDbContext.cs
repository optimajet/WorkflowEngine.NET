using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Abp.EntityFramework;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AngularBPWorkflow.EntityFramework
{
    public class AngularBPWorkflowDbContext : AbpDbContext
    {
       public DbSet<WorkflowScheme> Tasks { get; set; }

        public AngularBPWorkflowDbContext(DbContextOptions<AngularBPWorkflowDbContext> options)
            : base(options)
        {

        }
    }
}
