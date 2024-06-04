using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // go to link https://localhost:7050/Projects/Add to add more project
        [HttpPost]
        public async Task<IActionResult> Add(AddProjectViewModel viewModel)
        {
            var project = new Project
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                CreateAt = viewModel.CreatedAt,
                UpdateAt = viewModel.UpdatedAt
            };

            await dbContext.Projects.AddAsync(project);
            await dbContext.SaveChangesAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var projects = await dbContext.Projects.ToListAsync();

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await dbContext.Projects.FindAsync(id);

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Project viewModel)
        {
            var project = await dbContext.Projects.FindAsync(viewModel.Project_id);

            if (project is not null)
            {
                project.Name = viewModel.Name;
                project.Description = viewModel.Description;
                project.CreateAt = viewModel.CreateAt;
                project.UpdateAt = viewModel.UpdateAt;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Projects");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Project viewModel)
        {
            var project = await dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Project_id == viewModel.Project_id);

            if (project is not null)
            {
                dbContext.Projects.Remove(viewModel);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Projects");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await dbContext.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedUser)
                //.ThenInclude(t => t.) // Bao gồm thông tin người dùng được giao nhiệm vụ
                .FirstOrDefaultAsync(p => p.Project_id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

    }
}

