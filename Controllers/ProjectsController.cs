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
        private readonly UserManagementProxy _userManagementProxy;
        private readonly IProjectFactory projectFatory;

        public ProjectsController(ApplicationDbContext dbContext, UserManagementProxy userManagementProxy, IProjectFactory factory)
        {
            // Khởi tạo các thành viên và kiểm tra null
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userManagementProxy = userManagementProxy ?? throw new ArgumentNullException(nameof(userManagementProxy));
            projectFatory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        [HttpGet]
        [Authorize(Roles = "admin, manager")]
        public IActionResult Add()
        {
            // Trả về view để thêm dự án mới
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Add(AddProjectViewModel viewModel)
        {
            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Kiểm tra nếu projectFatory là null
                if (projectFatory == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Project factory is not initialized");
                }

                // Tạo dự án mới thông qua factory
                IProjects project = projectFatory.createuser(viewModel.UserId, viewModel.Name, viewModel.Description);
                var _project = (Project)project;
                await dbContext.Projects.AddAsync(_project);
                await dbContext.SaveChangesAsync();

                // Chuyển hướng đến trang chi tiết dự án
                return RedirectToAction("Details", "Projects", new { id = _project.Project_id });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the project");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Project viewModel)
        {
            var project = await dbContext.Projects.FindAsync(viewModel.Project_id);

            // Kiểm tra nếu dự án không tồn tại
            if (project == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin dự án
            project.Name = viewModel.Name;
            project.Description = viewModel.Description;
            project.UpdateAt = DateTime.Now;

            await dbContext.SaveChangesAsync();

            // Chuyển hướng đến trang chi tiết dự án sau khi chỉnh sửa
            return RedirectToAction("Details", "Projects", new { id = project.Project_id });
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await dbContext.Projects.FindAsync(id);

            // Kiểm tra nếu dự án không tồn tại
            if (project == null)
            {
                return NotFound();
            }

            // Xóa các task liên quan
            var relatedTasks = dbContext.Task.Where(t => t.Project_Id == id);
            dbContext.Task.RemoveRange(relatedTasks);

            // Xóa dự án
            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Kiểm tra nếu ID project không tồn tại
            var project = await dbContext.Projects
                .Include(p => p.Task)
                .ThenInclude(t => t.AssignedUser)
                .FirstOrDefaultAsync(p => p.Project_id == id);

            if (project == null)
            {
                return View("NotFound"); // Trả về view NotFound nếu không tìm thấy project
            }

            var currentUser = await _userManagementProxy.GetCurrentUserAsync();

            // Kiểm tra nếu người dùng là admin
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
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

                return View(project);
            }

            // Cuối cùng, nếu là employee thì chỉ có thể xem những dự án mà họ có task
            if (project.Task.Any(t => t.AssignedUser.Id == currentUser.Id))
            {
                return View(project);
            }

            return Forbid(); // Chuyển hướng đến trang 403 Forbidden nếu không có task của employee đó
        }
    }
}