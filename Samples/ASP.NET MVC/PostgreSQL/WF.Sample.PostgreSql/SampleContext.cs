using OptimaJet.Workflow.Core.Persistence;

namespace WF.Sample.PostgreSql
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity;


    public partial class SampleContext : DbContext
    {
        public SampleContext()
            : base("name=ConnectionString")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StructDivision> StructDivisions { get; set; }
        public virtual DbSet<Head> VHeads { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<WorkflowScheme> WorkflowSchemes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Documents)
                .WithRequired(e => e.Author)
                .HasForeignKey(e => e.AuthorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Documents1)
                .WithOptional(e => e.Manager)
                .HasForeignKey(e => e.ManagerId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.EmployeeRoles)
                .WithRequired(r => r.Employee)
                .HasForeignKey(r => r.EmployeeId);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.EmployeeRoles)
                .WithRequired(r => r.Role)
                .HasForeignKey(r => r.RoleId);

            modelBuilder.Entity<StructDivision>()
                .HasMany(e => e.StructDivision1)
                .WithOptional(e => e.StructDivision2)
                .HasForeignKey(e => e.ParentId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<EmployeeRole>()
                .HasKey(x => new { x.RoleId, x.EmployeeId })
                .HasRequired(x => x.Role).WithMany(x => x.EmployeeRoles)
         ;

        }
    }
}
