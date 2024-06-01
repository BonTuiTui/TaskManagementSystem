using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Models
{
    public class Notification
    {
        [Key]
        public int Notifications_id { get; set; }

        public string User_id { get; set; } // Change to string

        public int? Task_id { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreateAt { get; set; }

        [ForeignKey("User_id")]
        public virtual ApplicationUser User { get; set; } // Navigation property
    }
}