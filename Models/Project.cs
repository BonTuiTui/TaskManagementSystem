﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Interfaces;

namespace TaskManagementSystem.Models
{
    public class Project : IProjects
    {
        [Key]
        public int Project_id { get; set; }

        public string User_id { get; set; } // ID của người tạo dự án

        public string Description { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public Project(string User_id, string Name, string Description, DateTime CreateAt, DateTime UpdateAt)
        {
            this.User_id = User_id;
            this.Name = Name;
            this.Description = Description;
            this.CreateAt = CreateAt;
            this.UpdateAt = UpdateAt;
        }

        public Project() { }

        [ForeignKey("User_id")]
        public virtual ApplicationUser User { get; set; } // Navigation property

        public ICollection<Task> Task { get; set; }

        // Thêm thuộc tính cho danh sách thành viên
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

        void IProjects.annouce()
        {
            Console.WriteLine("vinh ne kkk");
        }
    }
}