using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Services;
using TaskManagementSystem.ViewModels;
using Task = TaskManagementSystem.Models.Task;
using TaskManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IUserManagementService _userManagementService;

        public TasksController(ApplicationDbContext dbContext, IUserManagementService userManagementService)
        {
            this.dbContext = dbContext;
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public IActionResult Add(int projectId, string status)
        {
            var viewModel = new AddTaskViewModel
            {
                ProjectId = projectId,
                Status = status
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await dbContext.Task
                .Where(t => t.Task_id == id)
                .Select(t => new
                {
                    t.Task_id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.AssignedTo,
                    t.DueDate,
                    t.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            var user = await _userManagementService.GetUserByIdAsync(task.AssignedTo);
            string assignedToUsername = user != null ? user.UserName : null;

            // Tạo một object mới chứa thông tin task và Username
            var result = new
            {
                task.Task_id,
                task.Title,
                task.Description,
                task.Status,
                AssignedToUsername = assignedToUsername,
                task.DueDate,
                task.UpdateAt
            };

            return Json(result);
        }


        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Add(AddTaskViewModel viewModel)
        {
            ApplicationUser assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                if (viewModel.AssignedTo.Contains("@"))
                {
                    assignedUser = await _userManagementService.GetUserByEmailAsync(viewModel.AssignedTo);
                }
                else
                {
                    assignedUser = await _userManagementService.GetUserByNameAsync(viewModel.AssignedTo);
                }

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

            await dbContext.Task.AddAsync(task);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var tasks = await dbContext.Task.ToListAsync();

            return View(tasks);
        }

        [HttpGet]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await dbContext.Task.FindAsync(id);

            // Check if task is null
            if (task == null)
            {
                return NotFound();
            }

            // Get the username from AssignedTo (userId)
            if (!string.IsNullOrEmpty(task.AssignedTo))
            {
                var user = await _userManagementService.GetUserByIdAsync(task.AssignedTo);
                if (user != null)
                {
                    ViewData["AssignedUserName"] = user.UserName;
                }
            }

            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Edit(int id, AddTaskViewModel viewModel)
        {
            var task = await dbContext.Task.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ApplicationUser assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                if (viewModel.AssignedTo.Contains("@"))
                {
                    assignedUser = await _userManagementService.GetUserByEmailAsync(viewModel.AssignedTo);
                }
                else
                {
                    assignedUser = await _userManagementService.GetUserByNameAsync(viewModel.AssignedTo);
                }

                if (assignedUser == null)
                {
                    return BadRequest("The specified user does not exist.");
                }
            }

            task.Title = viewModel.Title;
            task.Description = viewModel.Description;
            task.Status = viewModel.Status;
            task.AssignedTo = assignedUser?.Id;
            task.UpdateAt = DateTime.Now;  // Update the timestamp
            task.DueDate = viewModel.DueDate;

            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await dbContext.Task.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            dbContext.Task.Remove(task);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult UpdateStatus([FromBody] UpdateStatusModel model)
        {
            var task = dbContext.Task.Find(model.TaskId);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = model.Status;
            task.UpdateAt = DateTime.Now;
            dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManagementService.GetCurrentUserAsync();
            var project = await dbContext.Projects
                .Include(p => p.Task)
                .ThenInclude(t => t.AssignedUser)
                .FirstOrDefaultAsync(p => p.Project_id == id);

            if (project == null)
            {
                return NotFound();
            }

            var isUserInProject = project.Task.Any(t => t.AssignedTo == user.Id);
            if (!isUserInProject)
            {
                return Forbid("Bạn không có quyền truy cập vào dự án này.");
            }

            return View(project);
        }

        public class UpdateStatusModel
        {
            public int TaskId { get; set; }
            public string Status { get; set; }
        }
    }
}
