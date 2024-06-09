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
        [Authorize]
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
            var project = await dbContext.Projects
                .Include(p => p.Task)
                .Include(p => p.ProjectMembers) // Bao gồm ProjectMembers để xóa
                .FirstOrDefaultAsync(p => p.Project_id == id);

            if (project == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
            }

            // Xóa các task liên quan
            var relatedTasks = dbContext.Task.Where(t => t.Project_Id == id);
            dbContext.Task.RemoveRange(relatedTasks);

            // Xóa các ProjectMember liên quan
            var relatedProjectMembers = dbContext.ProjectMembers.Where(pm => pm.ProjectId == id);
            dbContext.ProjectMembers.RemoveRange(relatedProjectMembers);

            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // Kiểm tra nếu ID project không tồn tại
            var project = await dbContext.Projects
                .Include(p => p.Task)
                .ThenInclude(t => t.AssignedUser)
                .Include(p => p.ProjectMembers) // Bao gồm ProjectMembers để kiểm tra quyền truy cập
                .ThenInclude(pm => pm.User) // Bao gồm User trong ProjectMembers
                .FirstOrDefaultAsync(p => p.Project_id == id);

            if (project == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
            }

            var currentUser = await _userManagementProxy.GetCurrentUserAsync();
            var createdByUser = await _userManagementProxy.GetUserByIdAsync(project.User_id);

            // Kiểm tra nếu người dùng là admin
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                ViewBag.CreatedByUsername = createdByUser?.UserName;
                // Admin có thể xem toàn bộ dự án
                return View(project);
            }

            // Nếu không phải admin, kiểm tra tiếp nếu là manager
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "manager"))
            {
                // Manager chỉ có thể xem những dự án mà họ tạo ra
                if (project.User_id != currentUser.Id)
                {
                    return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không phải của manager đó
                }

                ViewBag.CreatedByUsername = createdByUser?.UserName;
                return View(project);
            }

            // Nếu người dùng là thành viên của dự án, hiển thị các project mà họ là thành viên
            if (project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id))
            {
                ViewBag.CreatedByUsername = createdByUser?.UserName;
                return View(project);
            }

            // Nếu không phải là admin, manager hoặc thành viên của dự án, không được truy cập
            return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không có quyền truy cập
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> AddUserToProject(int projectId, string userName)
        {
            var project = await dbContext.Projects.Include(p => p.ProjectMembers).FirstOrDefaultAsync(p => p.Project_id == projectId);
            if (project == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
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

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemoveUserFromProject(int projectId, string userId)
        {
            var project = await dbContext.Projects.Include(p => p.ProjectMembers).FirstOrDefaultAsync(p => p.Project_id == projectId);
            if (project == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
            }

            var projectMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId);
            if (projectMember == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
            }

            var tasksAssignedToUser = await dbContext.Task
                .Where(t => t.Project_Id == projectId && t.AssignedTo == userId)
                .Select(t => new { t.Title })
                .ToListAsync();

            if (tasksAssignedToUser.Any())
            {
                var taskTitles = string.Join(", ", tasksAssignedToUser.Select(t => t.Title));
                return BadRequest($"Cannot remove user. The user is assigned to tasks: {taskTitles}");
            }

            dbContext.ProjectMembers.Remove(projectMember);
            await dbContext.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            var projectMembers = await dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => new
                {
                    pm.User.Id,
                    pm.User.UserName,
                    pm.User.Email,
                    pm.User.FullName
                })
                .ToListAsync();

            return Json(projectMembers);
        }

    }
}