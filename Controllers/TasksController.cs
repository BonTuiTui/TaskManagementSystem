using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.ViewModels;
using Task = TaskManagementSystem.Models.Task;
using TaskManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskManagementSystem.Hubs;
using TaskManagementSystem.Services.Observer;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services.Proxies;

namespace TaskManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TaskNotifier _taskNotifier;
        private readonly UserManagementProxy _userManagementProxy;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TasksController(ApplicationDbContext dbContext, TaskNotifier taskNotifier, UserManagementProxy userManagementProxy, IHubContext<NotificationHub> hubContext)
        {
            _dbContext = dbContext;
            _taskNotifier = taskNotifier;
            _userManagementProxy = userManagementProxy;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _dbContext.Task
                .Where(t => t.Task_id == id)
                .Select(t => new
                {
                    t.Task_id,
                    t.Title,
                    t.Description,
                    t.Status,
                    AssignedToId = t.AssignedTo, 
                    t.DueDate,
                    t.UpdateAt,
                    AssignedUser = t.AssignedUser.UserName,
                    currentUsername = _userManagementProxy.GetCurrentUserAsync().Result.UserName
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            return Json(task);
        }

        [HttpGet]
        public async Task<IActionResult> AdvancedSearchTasks(string title, string status, string assignee, DateTime? dueDate)
        {
            var query = _dbContext.Task.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(t => t.Title.Contains(title));
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            if (!string.IsNullOrEmpty(assignee))
            {
                query = query.Where(t => t.AssignedUser.UserName.Contains(assignee));
            }
            if (dueDate.HasValue)
            {
                query = query.Where(t => t.DueDate.Date == dueDate.Value.Date);
            }

            var tasks = await query.Select(t => new
            {
                t.Task_id,
                t.Title,
                t.Description,
                t.Project_Id // Assuming ProjectId is part of the Task entity
            }).ToListAsync();

            return Json(tasks);
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Add(AddTaskViewModel viewModel)
        {
            ApplicationUser assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                assignedUser = await _userManagementProxy.GetUserByIdAsync(viewModel.AssignedTo);

                if (assignedUser == null)
                {
                    return BadRequest("The specified user does not exist.");
                }
            }

            var task = new Task
            {
                Project_Id = viewModel.ProjectId,
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status,
                AssignedTo = assignedUser?.Id,
                CreateAt = viewModel.CreatedAt,
                UpdateAt = DateTime.Now,
                DueDate = viewModel.DueDate
            };

            await _dbContext.Task.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Gửi thông báo tới user
            if (assignedUser != null)
            {
                string message = $"You have been assigned a new task: {task.Title}";
                _taskNotifier.Attach(new TaskObserver(assignedUser.Id, _dbContext));
                _taskNotifier.Notify(message);
                await _hubContext.Clients.User(assignedUser.Id).SendAsync("ReceiveNotification", $"Bạn đã được giao một task mới: {task.Title}");
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Edit(int id, AddTaskViewModel viewModel)
        {
            var task = await _dbContext.Task.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ApplicationUser assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                assignedUser = await _userManagementProxy.GetUserByIdAsync(viewModel.AssignedTo);

                if (assignedUser == null)
                {
                    return BadRequest("The specified user does not exist.");
                }
            }

            task.Title = viewModel.Title;
            task.Description = viewModel.Description;
            task.Status = viewModel.Status;
            task.AssignedTo = assignedUser?.Id;
            task.UpdateAt = DateTime.Now;  // Cập nhật thời gian
            task.DueDate = viewModel.DueDate;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // Phương thức POST để xóa một task theo ID
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _dbContext.Task.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            _dbContext.Task.Remove(task);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // Phương thức POST để cập nhật trạng thái của một task
        [HttpPost]
        public IActionResult UpdateStatus([FromBody] UpdateStatusModel model)
        {
            var task = _dbContext.Task.Find(model.TaskId);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = model.Status;
            task.UpdateAt = DateTime.Now;
            _dbContext.SaveChanges();

            return Ok();
        }
    }
}