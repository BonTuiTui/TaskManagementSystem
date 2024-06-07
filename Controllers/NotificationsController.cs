using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get notifications for a user
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.User_id == userId)
                .OrderByDescending(n => n.CreateAt)
                .ToListAsync();

            return Json(notifications);
        }

        // Mark notification as read
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            Console.WriteLine("MarkAsRead method called with notificationId: " + notificationId);

            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                Console.WriteLine("Notification marked as read.");
                return Ok();
            }
            Console.WriteLine("Notification not found with ID: " + notificationId);
            return NotFound();
        }


        // Create notification when a new comment is added
        public async Task<IActionResult> CreateNotificationForComment(TaskComment taskComment, string receiverUser)
        {
            var notification = new Notification
            {
                User_id = taskComment.User_id,
                Notification_text = $"New comment on task {taskComment.Task_id}: {taskComment.Comment_text}",
                CreateAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
