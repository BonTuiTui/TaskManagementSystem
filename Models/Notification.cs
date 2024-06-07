using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Models
{
    public class Notification
    {
        [Key]
        public int Notification_id { get; set; }

        public string User_id { get; set; } // User who will receive the notification
        public string Notification_text { get; set; } // The notification content
        public DateTime CreateAt { get; set; } // When the notification was created
        public bool IsRead { get; set; } // Whether the notification has been read

        [ForeignKey("User_id")]
        public virtual ApplicationUser User { get; set; } // Navigation property
    }
}
