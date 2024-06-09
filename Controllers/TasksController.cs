using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;
using Task = TaskManagementSystem.Models.Task;
using TaskManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManagementProxy _userManagementProxy;

        // Khởi tạo TasksController với ApplicationDbContext và UserManagementProxy
        public TasksController(ApplicationDbContext dbContext, UserManagementProxy userManagementProxy)
        {
            // Khởi tạo các thành viên và kiểm tra null
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userManagementProxy = userManagementProxy ?? throw new ArgumentNullException(nameof(userManagementProxy));
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
                    AssignedToId = t.AssignedTo, // Return the user ID instead of username
                    t.DueDate,
                    t.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            return Json(task);
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

            await dbContext.Task.AddAsync(task);
            await dbContext.SaveChangesAsync();

            return Ok();
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

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        // Phương thức POST để xóa một task theo ID
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

        // Phương thức POST để cập nhật trạng thái của một task
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

        // Lớp model để cập nhật trạng thái của một task
        public class UpdateStatusModel
        {
            public int TaskId { get; set; }
            public string Status { get; set; }
        }
    }
}