using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Models
{
    public class TaskComment
    {
        [Key]
        public int Comment_id { get; set; }

        public int? Task_id { get; set; }

        public string User_id { get; set; } // Change to string

        public string Comment_text { get; set; }

        public DateTime CreateAt { get; set; }

        [ForeignKey("User_id")]
        public virtual ApplicationUser? User { get; set; } // Navigation property
    }
}