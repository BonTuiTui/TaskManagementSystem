using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Models
{
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; } // Khóa chính cho bảng trung gian

        [Required]
        public int ProjectId { get; set; } // Khóa ngoại tới Project

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } // Thuộc tính dẫn hướng tới Project

        [Required]
        public string UserId { get; set; } // Khóa ngoại tới ApplicationUser

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Thuộc tính dẫn hướng tới ApplicationUser
    }
}