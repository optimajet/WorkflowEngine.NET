namespace WF.Sample.Oracle
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using global::Oracle.ManagedDataAccess.Client;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Configuration;

    public partial class SampleContext : DbContext
    {
        private readonly IConfiguration _config;

        public SampleContext(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<DocumentTransitionHistory> DocumentTransitionHistories { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StructDivision> StructDivisions { get; set; }
        public virtual DbSet<Head> VHeads { get; set; }
        public virtual DbSet<WorkflowInbox> WorkflowInboxes { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<WorkflowScheme> WorkflowSchemes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseOracle(_config.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = new OracleConnectionStringBuilder(_config.GetConnectionString("DefaultConnection"));
            modelBuilder.HasDefaultSchema(builder.UserID);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Name.ToUpper();
                }
            }

            
            modelBuilder.Entity<Document>().Property(x => x.Number)
                .HasColumnName("Number")
            ;
            modelBuilder.Entity<Document>().Property(x => x.Comment)
                .HasColumnName("Comment")
            ;
            modelBuilder.Entity<DocumentTransitionHistory>().Property(x => x.Order)
                .HasColumnName("Order")
            ;
   
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Documents)
                .WithOne(e => e.Author)
                .HasForeignKey(e => e.AuthorId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Documents1)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.EmployeeRoles)
                .WithOne(r => r.Employee)
                .HasForeignKey(r => r.EmployeeId);

            modelBuilder.Entity<Employee>()
             .Property(x => x.IsHead).HasConversion(x => x ? "1" : "0", x => x != "0");

            modelBuilder.Entity<Role>()
                        .HasMany(e => e.EmployeeRoles)
                        .WithOne(r => r.Role)
                        .HasForeignKey(r => r.RoleId);

            modelBuilder.Entity<StructDivision>()
                .HasMany(e => e.StructDivision1)
                .WithOne(e => e.StructDivision2)
                .HasForeignKey(e => e.ParentId)
            ;

            modelBuilder.Entity<EmployeeRole>()
                .HasKey(x => new { x.RoleId, x.EmployeeId })
            ;

            modelBuilder.Entity<EmployeeRole>()
             .HasOne(x => x.Role).WithMany(x => x.EmployeeRoles)
         ;


        }
    }
}
