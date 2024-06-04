using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManagementProxy _userManagementProxy; // Thêm UserManagementProxy

        public ProjectsController(ApplicationDbContext dbContext, UserManagementProxy userManagementProxy) // Thêm UserManagementProxy vào constructor
        {
            this.dbContext = dbContext;
            _userManagementProxy = userManagementProxy; // Khởi tạo UserManagementProxy
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
            var project = new Project
            {
                User_id = viewModel.UserId,
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
            var currentUser = await _userManagementProxy.GetCurrentUserAsync();

            // Kiểm tra nếu người dùng là admin
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                // Admin có thể xem toàn bộ dự án
                var project = await dbContext.Projects
                    .Include(p => p.Tasks)
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
                    .Include(p => p.Tasks)
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
                .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedUser)
                .FirstOrDefaultAsync(p => p.Project_id == id && p.Tasks.Any(t => t.AssignedUser.Id == currentUser.Id));

            if (projectAsEmployee == null)
            {
                return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không tìm thấy dự án hoặc không có task của employee đó
            }

            return View(projectAsEmployee);
        }

    }
}

