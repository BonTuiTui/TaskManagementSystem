using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Proxies; // Import proxy class

namespace TaskManagementSystem.ViewComponents
{
    [ViewComponent(Name = "Projects")]
    public class ProjectsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManagementProxy _userManagementProxy; // Proxy injection

        public ProjectsViewComponent(ApplicationDbContext applicationDbContext, UserManagementProxy userManagementProxy)
        {
            _context = applicationDbContext;
            _userManagementProxy = userManagementProxy; // Assign proxy instance
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManagementProxy.GetCurrentUserAsync(); // Get current user via proxy

            if (currentUser == null)
            {
                // Nếu không có người dùng hiện tại, trả về danh sách trống
                return View("Projects", new List<Project>());
            }

            List<Project> projectsToShow = new List<Project>();

            // Kiểm tra vai trò của người dùng
            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                // Nếu người dùng là Admin hiển thị tất cả các project
                projectsToShow = _context.Projects.ToList();
            } else if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "manager"))
            {
                // Nếu người dùng là Manager hiển thị tất cả các project
                projectsToShow = _context.Projects
                    .Where(p => p.User_id == currentUser.Id)
                    .ToList();
            }
            else
            {
                // Nếu người dùng là Employee, chỉ hiển thị các project mà họ được giao task
                projectsToShow = _context.Projects
                    .Where(p => p.Task.Any(t => t.AssignedTo == currentUser.Id))
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
