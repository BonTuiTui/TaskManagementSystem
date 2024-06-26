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

            builder.Entity<IdentityRole>().HasData(adminRole, managerRole, employeeRole);

            builder.Entity<Project>().ToTable("Project");
            builder.Entity<Task>().ToTable("Task");
            builder.Entity<Notification>().ToTable("Notification");
            builder.Entity<TaskComment>().ToTable("TaskComment");

            builder.Entity<ProjectMember>().ToTable("ProjectMember");
            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Task)
                .HasForeignKey(t => t.Project_Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskComment>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(tc => tc.Task_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Project>()
                .HasOne<ApplicationUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Task>()
                .HasOne<ApplicationUser>(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Task { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
    }
}