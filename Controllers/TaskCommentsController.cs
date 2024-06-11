using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TaskCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskComments(int taskId)
        {
            var taskComments = await _context.TaskComments
                .Where(c => c.Task_id == taskId)
                .Select(c => new
                {
                    c.Comment_text,
                    c.CreateAt,
                    UserName = c.User.UserName
                })
                .ToListAsync();

            return Json(taskComments);
        }

        [HttpPost]
        public async Task<IActionResult> AddTaskComment(TaskComment taskComment)
        {
            Console.WriteLine("AddTaskComment started");

            if (ModelState.IsValid)
            {
                taskComment.CreateAt = DateTime.UtcNow;
                _context.TaskComments.Add(taskComment);
                await _context.SaveChangesAsync();
                Console.WriteLine("Task comment saved");

                var task = await _context.Task
                    .Include(t => t.Project)
                    .Include(t => t.AssignedUser)
                    .FirstOrDefaultAsync(t => t.Task_id == taskComment.Task_id);

                if (task == null)
                {
                    Console.WriteLine("Task not found");
                    return NotFound("Task not found");
                }

                Console.WriteLine($"Task found: {task.Task_id}");

                var notifications = new List<Notification>();
                var userName = User.Identity.Name;
                var userRole = User.IsInRole("admin") ? "Admin" : User.IsInRole("manager") ? "Manager" : "Employee";

                if (User.IsInRole("employee"))
                {
                    Console.WriteLine("User role detected");
                    var managerId = task.Project?.User_id;
                    if (managerId != null)
                    {
                        notifications.Add(new Notification
                        {
                            User_id = managerId,
                            Notification_text = $"New comment on your task (Task ID: {task.Task_id} - <a href='/Projects/Details/{task.Project_Id}?taskId={task.Task_id}' target='_blank'>View Task</a>) from {userName} ({userRole}): {taskComment.Comment_text}",
                            CreateAt = DateTime.UtcNow
                        });
                        Console.WriteLine($"Notification for manager {managerId} added");
                    }
                    else
                    {
                        Console.WriteLine("Manager ID is null");
                    }
                }
                else if (User.IsInRole("manager"))
                {
                    Console.WriteLine("Manager role detected");
                    var userId = task.AssignedUser?.Id;
                    if (userId != null)
                    {
                        notifications.Add(new Notification
                        {
                            User_id = userId,
                            Notification_text = $"New comment on your task (Task ID: {task.Task_id} - <a href='/Projects/Details/{task.Project_Id}?taskId={task.Task_id}' target='_blank'>View Task</a>) from {userName} ({userRole}): {taskComment.Comment_text}",
                            CreateAt = DateTime.UtcNow
                        });
                        Console.WriteLine($"Notification for user {userId} added");
                    }
                    else
                    {
                        Console.WriteLine("User ID is null");
                    }
                }
                else if (User.IsInRole("admin"))
                {
                    Console.WriteLine("Admin role detected");
                    var managerId = task.Project?.User_id;
                    var userId = task.AssignedUser?.Id;

                    if (managerId != null)
                    {
                        notifications.Add(new Notification
                        {
                            User_id = managerId,
                            Notification_text = $"New comment on a task in your project (Task ID: {task.Task_id} - <a href='/Projects/Details/{task.Project_Id}?taskId={task.Task_id}' target='_blank'>View Task</a>) from {userName} ({userRole}): {taskComment.Comment_text}",
                            CreateAt = DateTime.UtcNow
                        });
                        Console.WriteLine($"Notification for manager {managerId} added");
                    }
                    else
                    {
                        Console.WriteLine("Manager ID is null");
                    }

                    if (userId != null)
                    {
                        notifications.Add(new Notification
                        {
                            User_id = userId,
                            Notification_text = $"New comment on your task (Task ID: {task.Task_id} - <a href='/Projects/Details/{task.Project_Id}?taskId={task.Task_id}' target='_blank'>View Task</a>) from {userName} ({userRole}): {taskComment.Comment_text}",
                            CreateAt = DateTime.UtcNow
                        });
                        Console.WriteLine($"Notification for user {userId} added");
                    }
                    else
                    {
                        Console.WriteLine("User ID is null");
                    }
                }

                if (notifications.Any())
                {
                    _context.Notifications.AddRange(notifications);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Notifications saved to database");
                }
                else
                {
                    Console.WriteLine("No notifications to save");
                }

                return Ok();
            }
            else
            {
                Console.WriteLine("ModelState is invalid");
                return BadRequest(ModelState);
            }
        }
    }
}