using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Proxies; // Import lớp proxy

namespace TaskManagementSystem.ViewComponents
{
    // Đặt tên cho ViewComponent là "Projects"
    [ViewComponent(Name = "Projects")]
    public class ProjectsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManagementProxy _userManagementProxy; // Proxy injection

        // Constructor nhận ApplicationDbContext và UserManagementProxy thông qua Dependency Injection
        public ProjectsViewComponent(ApplicationDbContext applicationDbContext, UserManagementProxy userManagementProxy)
        {
            _context = applicationDbContext;
            _userManagementProxy = userManagementProxy; // Gán instance của proxy
        }

        // Phương thức InvokeAsync là điểm vào của ViewComponent
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy người dùng hiện tại thông qua proxy
            var currentUser = await _userManagementProxy.GetCurrentUserAsync();

            if (currentUser == null)
            {
                // Nếu không có người dùng hiện tại, trả về danh sách trống
                return View("Projects", new List<Project>());
            }

            List<Project> projectsToShow = new List<Project>();

            // Kiểm tra vai trò của người dùng
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                // Nếu người dùng là Admin, hiển thị tất cả các project
                projectsToShow = _context.Projects.ToList();
            }
            else if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "manager"))
            {
                // Nếu người dùng là Manager, hiển thị các project mà họ tạo ra
                projectsToShow = _context.Projects
                    .Where(p => p.User_id == currentUser.Id)
                    .ToList();
            }
            else
            {
                // Nếu người dùng là thành viên của dự án, hiển thị các project mà họ là thành viên
                projectsToShow = _context.Projects
                    .Where(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id))
                    .ToList();
            }

            // In ra console các project mà người dùng có quyền xem
            foreach (var project in projectsToShow)
            {
                Console.WriteLine($"User {currentUser.UserName} can access project: {project.Name}");
            }

            // Trả về danh sách các project mà người dùng có quyền xem cho view
            return View("Projects", projectsToShow);
        }
    }
}