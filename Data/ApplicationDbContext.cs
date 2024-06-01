﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Models;
using Task = TaskManagementSystem.Models.Task;

namespace TaskManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define roles
            var adminRole = new IdentityRole
            {
                Id = "1",
                Name = "admin",
                NormalizedName = "ADMIN"
            };

            var managerRole = new IdentityRole
            {
                Id = "2",
                Name = "manager",
                NormalizedName = "MANAGER"
            };

            var employeeRole = new IdentityRole
            {
                Id = "3",
                Name = "employee",
                NormalizedName = "EMPLOYEE"
            };

            // Seed roles
            builder.Entity<IdentityRole>().HasData(adminRole, managerRole, employeeRole);

            // Configure your existing tables
            builder.Entity<Project>().ToTable("Projects");
            builder.Entity<Task>().ToTable("Task");
            builder.Entity<Notification>().ToTable("Notifications");
            builder.Entity<TaskComment>().ToTable("TaskComments");

            // Define foreign key relationships
            builder.Entity<Task>()
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(t => t.Project_Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(n => n.Task_id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskComment>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(tc => tc.Task_id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Project>()
                .HasOne<ApplicationUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.User_id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Task>()
                .HasOne<ApplicationUser>(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
    }
}