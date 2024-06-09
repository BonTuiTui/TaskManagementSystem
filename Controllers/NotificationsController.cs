using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationsCount(string userId)
        {
            var unreadCount = await _context.Notifications
                .Where(n => n.User_id == userId && !n.IsRead)
                .CountAsync();

            return Json(unreadCount);
        }

        // Phương thức GET để lấy thông báo của người dùng
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            // Lấy thông báo của người dùng theo userId và sắp xếp theo thời gian tạo giảm dần
            var notifications = await _context.Notifications
                .Where(n => n.User_id == userId)
                .OrderByDescending(n => n.CreateAt)
                .ToListAsync();

            return Json(notifications);
        }

        // Phương thức POST để đánh dấu thông báo là đã đọc
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            Console.WriteLine("MarkAsRead method called with notificationId: " + notificationId);

            // Tìm thông báo theo notificationId
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true; // Đánh dấu thông báo là đã đọc
                await _context.SaveChangesAsync();
                Console.WriteLine("Notification marked as read.");
                return Ok();
            }
            Console.WriteLine("Notification not found with ID: " + notificationId);
            return NotFound();
        }

        // Phương thức tạo thông báo khi có bình luận mới được thêm vào
        [HttpPost]
        public async Task<IActionResult> CreateNotificationForComment(TaskComment taskComment, string receiverUser)
        {
            var notification = new Notification
            {
                User_id = receiverUser, // Đặt người nhận thông báo là receiverUser
                Notification_text = $"New comment on task {taskComment.Task_id}: {taskComment.Comment_text}",
                CreateAt = DateTime.UtcNow,
                IsRead = false
            };

            // Thêm thông báo mới vào cơ sở dữ liệu
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}