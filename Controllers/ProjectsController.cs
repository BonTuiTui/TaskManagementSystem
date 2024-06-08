using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.ProjectFactory;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManagementProxy _userManagementProxy; // Thêm UserManagementProxy
        private readonly IProjectFactory projectFactory;

        public ProjectsController(ApplicationDbContext dbContext, UserManagementProxy userManagementProxy, IProjectFactory factory) // Thêm UserManagementProxy vào constructor
        {
            this.dbContext = dbContext;
            _userManagementProxy = userManagementProxy; // Khởi tạo UserManagementProxy
            projectFactory = factory;
        }

        [HttpGet]
        [Authorize(Roles = "admin, manager")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Add(AddProjectViewModel viewModel)
        {
            if (projectFactory == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Project factory is not initialized");
            }

            IProjects project = projectFactory.createuser(viewModel.UserId, viewModel.Name, viewModel.Description);
            var _project = (Project)project;
            await dbContext.Projects.AddAsync(_project);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = _project.Project_id });
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
        [Authorize(Roles = "admin, manager")]
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
                    .Include(p => p.ProjectMembers) // Include ProjectMembers
                    .ThenInclude(pm => pm.User) // Include User in ProjectMembers
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
                    .Include(p => p.ProjectMembers) // Include ProjectMembers
                    .ThenInclude(pm => pm.User) // Include User in ProjectMembers
                    .FirstOrDefaultAsync(p => p.Project_id == id && p.User_id == currentUser.Id);

                if (project == null)
                {
                    return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không tìm thấy dự án hoặc không phải của manager đó
                }

                return View(project);
            }

            // Nếu là thành viên của dự án thì có thể xem dự án
            var projectAsMember = await dbContext.Projects
                .Include(p => p.Task)
                .ThenInclude(t => t.AssignedUser)
                .Include(p => p.ProjectMembers) // Include ProjectMembers
                .ThenInclude(pm => pm.User) // Include User in ProjectMembers
                .FirstOrDefaultAsync(p => p.Project_id == id && p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id));

            if (projectAsMember == null)
            {
                return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không tìm thấy dự án hoặc không phải là thành viên của dự án
            }

            return View(projectAsMember);
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> AddUserToProject(int projectId, string userName)
        {
            var project = await dbContext.Projects.Include(p => p.ProjectMembers).FirstOrDefaultAsync(p => p.Project_id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            var user = await _userManagementProxy.GetUserByNameAsync(userName) ?? await _userManagementProxy.GetUserByEmailAsync(userName);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (project.ProjectMembers.Any(pm => pm.UserId == user.Id))
            {
                return BadRequest("User is already a member of this project.");
            }

            project.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = user.Id });
            await dbContext.SaveChangesAsync();

            return Ok(new { userName = user.UserName, email = user.Email });
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            var projectMembers = await dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => new
                {
                    pm.User.Id,
                    pm.User.UserName
                })
                .ToListAsync();

            return Json(projectMembers);
        }
    }
}