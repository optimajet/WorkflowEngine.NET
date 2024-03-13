namespace WF.Sample.MySql
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Configuration;

    public partial class SampleContext : DbContext
    {
        private readonly IConfiguration _config;

        public SampleContext(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StructDivision> StructDivisions { get; set; }
        public virtual DbSet<Head> VHeads { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<WorkflowScheme> WorkflowSchemes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL(_config.GetConnectionString("DefaultConnection"));
            }
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .Property(x => x.Id).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<Employee>()
                .Property(x => x.StructDivisionId).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<Employee>()
                .Property(x => x.IsHead).HasConversion<int>();
                
            modelBuilder.Entity<Role>()
                        .HasMany(e => e.EmployeeRoles)
                        .WithOne(r => r.Role)
                        .HasForeignKey(r => r.RoleId);

            modelBuilder.Entity<Role>()
                .Property(x => x.Id).HasConversion(new GuidToBytesConverter());

            modelBuilder.Entity<StructDivision>()
                .HasMany(e => e.StructDivision1)
                .WithOne(e => e.StructDivision2)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<StructDivision>()
                .Property(x => x.Id).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<StructDivision>()
                .Property(x => x.ParentId).HasConversion(new GuidToBytesConverter());

            modelBuilder.Entity<EmployeeRole>()
                .HasKey(x => new { x.RoleId, x.EmployeeId })
            ;

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(x => x.Role).WithMany(x => x.EmployeeRoles)
            ;

            modelBuilder.Entity<EmployeeRole>()
               .Property(x => x.EmployeeId).HasConversion(new GuidToBytesConverter());

            modelBuilder.Entity<EmployeeRole>()
                .Property(x => x.RoleId).HasConversion(new GuidToBytesConverter());

            modelBuilder.Entity<Head>()
                .Property(x => x.Id).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<Head>()
                .Property(x => x.HeadId).HasConversion(new GuidToBytesConverter());

            modelBuilder.Entity<Document>()
                .Property(x => x.Id).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<Document>()
                .Property(x => x.AuthorId).HasConversion(new GuidToBytesConverter());
            modelBuilder.Entity<Document>()
                .Property(x => x.ManagerId).HasConversion(new GuidToBytesConverter());
        }
    }
}
