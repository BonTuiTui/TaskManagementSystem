using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Models
{
    public class Project
    {
        [Key]
        public int Project_id { get; set; }

        public string User_id { get; set; } // Change to string

        public string Description { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [ForeignKey("User_id")]
        public virtual ApplicationUser User { get; set; } // Navigation property

        public ICollection<Task> Task { get; set; }
    }
}