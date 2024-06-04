using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.ViewModels;
using Task = TaskManagementSystem.Models.Task;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagementSystem.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public TasksController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // go to link https://localhost:7050/Tasks/Add to add more tasks
        [HttpPost]
        public async Task<IActionResult> Add(AddTaskViewModel viewModel)
        {
            var task = new Task
            {
                Project_Id = viewModel.ProjectId,
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status,
                AssignedTo = viewModel.AssignedTo,
                CreateAt = viewModel.CreatedAt,
                UpdateAt = viewModel.UpdatedAt,
                DueDate = viewModel.DueDate
            };

            await dbContext.Tasks.AddAsync(task);
            await dbContext.SaveChangesAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var tasks = await dbContext.Tasks.ToListAsync();

            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await dbContext.Tasks.FindAsync(id);

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Task viewModel)
        {
            var task = await dbContext.Tasks.FindAsync(viewModel.Task_id);

            if (task is not null)
            {
                task.Project_Id = viewModel.Project_Id;
                task.Title = viewModel.Title;
                task.Description = viewModel.Description;
                task.Status = viewModel.Status;
                task.AssignedTo = viewModel.AssignedTo;
                task.CreateAt = viewModel.CreateAt;
                task.UpdateAt = viewModel.UpdateAt;
                task.DueDate = viewModel.DueDate;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Tasks");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Task viewModel)
        {
            var task = await dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Task_id == viewModel.Task_id);

            if (task is not null)
            {
                dbContext.Tasks.Remove(viewModel);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Tasks");
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] UpdateStatusModel model)
        {
            var task = dbContext.Tasks.Find(model.TaskId);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = model.Status;
            dbContext.SaveChanges();

            return Ok();
        }

        public class UpdateStatusModel
        {
            public int TaskId { get; set; }
            public string Status { get; set; }
        }
    }

}

