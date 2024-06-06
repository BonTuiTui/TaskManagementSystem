using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;
using TaskManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Task = TaskManagementSystem.Models.Task;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Proxies;

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

        [HttpPost]
        public async Task<IActionResult> Add(AddTaskViewModel viewModel)
        {
            ApplicationUser? assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                // Check if AssignedTo is a username or an email
                if (viewModel.AssignedTo.Contains("@"))
                {
                    // AssignedTo is an email
                    assignedUser = await _userManagementService.GetUserByEmailAsync(viewModel.AssignedTo);
                }
                else
                {
                    // AssignedTo is a username
                    assignedUser = await _userManagementService.GetUserByNameAsync(viewModel.AssignedTo);
                }

                // Check if the assigned user exists
                if (assignedUser == null)
                {
                    // Return an error message if the user does not exist
                    ModelState.AddModelError("AssignedTo", "The specified user does not exist.");
                    return View(viewModel);
                }
            }
            var task = new Task
            {
                Project_Id = viewModel.ProjectId,
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status,
                AssignedTo = assignedUser?.Id, // Assign the user's ID or null if not specified
                CreateAt = viewModel.CreatedAt,
                DueDate = viewModel.DueDate
            };

            await dbContext.Task.AddAsync(task);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = viewModel.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var tasks = await dbContext.Task.ToListAsync();

            return View(tasks);
        }

        [HttpGet]
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


        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var task = await dbContext.Task.FindAsync(id);

        //    return View(task);
        //}

        [HttpPost]
        public async Task<IActionResult> Edit(Task viewModel)
        {
            ApplicationUser? assignedUser = null;
            if (!string.IsNullOrEmpty(viewModel.AssignedTo))
            {
                // Check if AssignedTo is a username or an email
                if (viewModel.AssignedTo.Contains("@"))
                {
                    // AssignedTo is an email
                    assignedUser = await _userManagementService.GetUserByEmailAsync(viewModel.AssignedTo);
                }
                else
                {
                    // AssignedTo is a username
                    assignedUser = await _userManagementService.GetUserByNameAsync(viewModel.AssignedTo);
                }

                // Check if the assigned user exists
                if (assignedUser == null)
                {
                    // Return an error message if the user does not exist
                    ModelState.AddModelError("AssignedTo", "The specified user does not exist.");
                    return View(viewModel);
                }
            }

            var task = await dbContext.Task.FindAsync(viewModel.Task_id);

            if (task != null)
            {
                task.Project_Id = viewModel.Project_Id;
                task.Title = viewModel.Title;
                task.Description = viewModel.Description;
                task.Status = viewModel.Status;
                task.AssignedTo = assignedUser?.Id;
                task.UpdateAt = DateTime.Now;
                task.DueDate = viewModel.DueDate;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Projects", new { id = viewModel.Project_Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Task viewModel)
        {
            var task = await dbContext.Task
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Task_id == viewModel.Task_id);

            if (task != null)
            {
                dbContext.Task.Remove(viewModel);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Tasks");
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
