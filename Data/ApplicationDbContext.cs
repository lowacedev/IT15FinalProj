using Microsoft.EntityFrameworkCore;
using ITSMS.Models;

namespace ITSMS.Data
{
    /// <summary>
    /// Application Database Context for IT Service Management System
    /// Configures all entities and relationships using EF Core Fluent API
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        // ERP Modules
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetAssignment> AssetAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================== ROLE CONFIGURATION ========================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.Description).HasMaxLength(255);

                // Seed default roles
                entity.HasData(
                    new Role { RoleId = 1, RoleName = "Admin", Description = "IT Administrator with full access" },
                    new Role { RoleId = 2, RoleName = "Technician", Description = "IT Support Technician" },
                    new Role { RoleId = 3, RoleName = "Employee", Description = "Employee / Requestor" },
                    new Role { RoleId = 4, RoleName = "SuperAdmin", Description = "System Super Administrator with unrestricted access" }
                );
            });

            // ======================== USER CONFIGURATION ========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key relationship with Role
                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index for frequent queries
                entity.HasIndex(e => e.RoleId);

                // Seed SuperAdmin user
                var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                var superAdminUser = new User
                {
                    UserId = -1,
                    Username = "superadmin",
                    Email = "superadmin@itsms.local",
                    FirstName = "Super",
                    LastName = "Admin",
                    PhoneNumber = "",
                    RoleId = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                superAdminUser.PasswordHash = passwordHasher.HashPassword(superAdminUser, "superadmin123");
                entity.HasData(superAdminUser);
            });

            // ======================== CATEGORY CONFIGURATION ========================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Seed default categories
                entity.HasData(
                    new Category { CategoryId = 1, CategoryName = "Hardware", Description = "Hardware related issues and requests" },
                    new Category { CategoryId = 2, CategoryName = "Software", Description = "Software installation and support" },
                    new Category { CategoryId = 3, CategoryName = "Network", Description = "Network connectivity issues" },
                    new Category { CategoryId = 4, CategoryName = "Email", Description = "Email and collaboration tools" },
                    new Category { CategoryId = 5, CategoryName = "Security", Description = "Security related issues" },
                    new Category { CategoryId = 6, CategoryName = "Other", Description = "Other miscellaneous requests" }
                );
            });

            // ======================== SERVICE REQUEST CONFIGURATION ========================
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.RequestNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.RequestNumber).IsUnique();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(ServiceRequestStatus.Pending).HasConversion<string>();
                entity.Property(e => e.Priority).HasDefaultValue(ServiceRequestPriority.Medium).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key: Category
                entity.HasOne(sr => sr.Category)
                    .WithMany(c => c.ServiceRequests)
                    .HasForeignKey(sr => sr.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Foreign key: Requestor (User who created the request)
                entity.HasOne(sr => sr.Requestor)
                    .WithMany(u => u.RequestsCreated)
                    .HasForeignKey(sr => sr.RequestorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ServiceRequests_Requestor");

                // Foreign key: AssignedTechnician (nullable)
                entity.HasOne(sr => sr.AssignedTechnician)
                    .WithMany(u => u.RequestsAssigned)
                    .HasForeignKey(sr => sr.AssignedTechnicianId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ServiceRequests_AssignedTechnician");

                // ERP Foreign Keys
                entity.HasOne(sr => sr.Asset)
                    .WithMany(a => a.ServiceRequests)
                    .HasForeignKey(sr => sr.AssetId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ServiceRequests_Asset");

                entity.HasOne(sr => sr.Employee)
                    .WithMany(e => e.ServiceRequests)
                    .HasForeignKey(sr => sr.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ServiceRequests_Employee");

                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.RequestorId);
                entity.HasIndex(e => e.AssignedTechnicianId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // ======================== ASSIGNMENT CONFIGURATION ========================
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId);
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Notes).HasMaxLength(255);

                // Foreign key: ServiceRequest
                entity.HasOne(a => a.Request)
                    .WithMany(sr => sr.Assignments)
                    .HasForeignKey(a => a.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key: Technician
                entity.HasOne(a => a.Technician)
                    .WithMany(u => u.AssignmentsReceived)
                    .HasForeignKey(a => a.TechnicianId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Assignments_Technician");

                // Foreign key: AssignedBy (Admin/Manager)
                entity.HasOne(a => a.AssignedByUser)
                    .WithMany(u => u.AssignmentsMade)
                    .HasForeignKey(a => a.AssignedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Assignments_AssignedBy");

                // Indexes
                entity.HasIndex(e => e.RequestId);
                entity.HasIndex(e => e.TechnicianId);
                entity.HasIndex(e => e.IsActive);
            });

            // ======================== FEEDBACK CONFIGURATION ========================
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.FeedbackId);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Comments).HasMaxLength(1000);
                entity.Property(e => e.ProvidedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // One-to-one relationship with ServiceRequest (unique)
                entity.HasOne(f => f.Request)
                    .WithOne(sr => sr.Feedback)
                    .HasForeignKey<Feedback>(f => f.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key: ProvidedBy
                entity.HasOne(f => f.User)
                    .WithMany(u => u.FeedbackProvided)
                    .HasForeignKey(f => f.ProvidedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Rating);
                entity.HasIndex(e => e.ProvidedAt);
            });

            // ======================== ACTIVITY LOG CONFIGURATION (OPTIONAL) ========================
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.Entity).HasMaxLength(50);
                entity.Property(e => e.Action).HasMaxLength(50);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.LoggedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign key: User (nullable)
                entity.HasOne(al => al.User)
                    .WithMany()
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.LoggedAt);
                entity.HasIndex(e => new { e.Entity, e.EntityId });
            });

            // ======================== ERP MODULES CONFIGURATION ========================
            
            // Department Configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                // Seed initial departments
                entity.HasData(
                    new Department { Id = 1, Name = "Information Technology" },
                    new Department { Id = 2, Name = "Human Resources" },
                    new Department { Id = 3, Name = "Finance" },
                    new Department { Id = 4, Name = "Operations" },
                    new Department { Id = 5, Name = "Marketing" }
                );
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeCode).HasMaxLength(20);
                entity.HasIndex(e => e.UserId).IsUnique(); // 1:1 with User

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Employee)
                    .HasForeignKey<Employee>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Asset Configuration
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssetTag).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.AssetTag).IsUnique();
                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(150);
            });

            // Asset Assignment Configuration
            modelBuilder.Entity<AssetAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(a => a.Asset)
                    .WithMany(ast => ast.Assignments)
                    .HasForeignKey(a => a.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(a => a.Employee)
                    .WithMany(emp => emp.AssetAssignments)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
