using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Models;
using Task = TaskManagementSystem.Models.Task;

namespace TaskManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor nhận các tùy chọn DbContextOptions và truyền chúng cho lớp cơ sở
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Override phương thức OnModelCreating để cấu hình mô hình dữ liệu
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Định nghĩa các vai trò
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

            // Seed các vai trò vào cơ sở dữ liệu
            builder.Entity<IdentityRole>().HasData(adminRole, managerRole, employeeRole);

            // Cấu hình các bảng hiện có
            builder.Entity<Project>().ToTable("Project");
            builder.Entity<Task>().ToTable("Task");
            builder.Entity<Notification>().ToTable("Notification");
            builder.Entity<TaskComment>().ToTable("TaskComment");

            // Định nghĩa các mối quan hệ khóa ngoại
            // Một Task thuộc về một Project, một Project có nhiều Task
            builder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Task)
                .HasForeignKey(t => t.Project_Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Một TaskComment thuộc về một Task, và khi Task bị xóa thì các TaskComment cũng bị xóa theo
            builder.Entity<TaskComment>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(tc => tc.Task_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Một Project thuộc về một ApplicationUser (User), và khi User bị xóa thì Project cũng bị xóa theo
            builder.Entity<Project>()
                .HasOne<ApplicationUser>(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.User_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Một Task thuộc về một ApplicationUser (AssignedUser), và khi AssignedUser bị xóa thì Task sẽ có trường AssignedTo null
            builder.Entity<Task>()
                .HasOne<ApplicationUser>(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);
        }

        // DbSet cho các bảng trong cơ sở dữ liệu
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Task { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
    }
}