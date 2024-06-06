using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.ProjectFactory;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManagementProxy _userManagementProxy; // Thêm UserManagementProxy
        private readonly IProjectFactory projectFatory; 

        public ProjectsController(ApplicationDbContext dbContext, UserManagementProxy userManagementProxy, IProjectFactory factory) // Thêm UserManagementProxy vào constructor
        {
            this.dbContext = dbContext;
            _userManagementProxy = userManagementProxy; // Khởi tạo UserManagementProxy
            projectFatory = factory;
        }

        [HttpGet]
        [Authorize(Roles = "admin, manager")]
        public IActionResult Add()
        {
            return View();
        }

        // go to link https://localhost:7050/Projects/Add to add more project
        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Add(AddProjectViewModel viewModel)
        {
            
            if (projectFatory == null)
            {
    // Handle the error appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, "Project factory is not initialized");
            }
 
                
            IProjects project = projectFatory.createuser(viewModel.UserId, viewModel.Name, viewModel.Description);
            var _project = (Project)project;
            await dbContext.Projects.AddAsync(_project);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = _project.Project_id });
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
                project.UpdateAt = DateTime.Now;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Projects", new { id = project.Project_id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            // Xóa các task liên quan
            var relatedTasks = dbContext.Task.Where(t => t.Project_Id == id);
            dbContext.Task.RemoveRange(relatedTasks);

            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync();

            return Ok();
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManagementProxy.GetCurrentUserAsync();

            // Kiểm tra nếu người dùng là admin
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                // Admin có thể xem toàn bộ dự án
                var project = await dbContext.Projects
                    .Include(p => p.Task)
                    .ThenInclude(t => t.AssignedUser)
                    .FirstOrDefaultAsync(p => p.Project_id == id);

                if (project == null)
                {
                    return NotFound();
                }

                return View(project);
            }

            // Nếu không phải admin, kiểm tra tiếp nếu là manager
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "manager"))
            {
                // Manager chỉ có thể xem những dự án mà họ tạo ra
                var project = await dbContext.Projects
                    .Include(p => p.Task)
                    .ThenInclude(t => t.AssignedUser)
                    .FirstOrDefaultAsync(p => p.Project_id == id && p.User_id == currentUser.Id);

                if (project == null)
                {
                    return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không tìm thấy dự án hoặc không phải của manager đó
                }

                return View(project);
            }

            // Cuối cùng, nếu là employee thì chỉ có thể xem những dự án mà họ có task
            var projectAsEmployee = await dbContext.Projects
                .Include(p => p.Task)
                .ThenInclude(t => t.AssignedUser)
                .FirstOrDefaultAsync(p => p.Project_id == id && p.Task.Any(t => t.AssignedUser.Id == currentUser.Id));

            if (projectAsEmployee == null)
            {
                return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không tìm thấy dự án hoặc không có task của employee đó
            }

            return View(projectAsEmployee);
        }

    }
}

