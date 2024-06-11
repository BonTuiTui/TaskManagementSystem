using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.ViewComponents
{
    [ViewComponent(Name = "Projects")]
    public class ProjectsViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;
        private readonly UserManagementProxy _userManagementProxy;

        public ProjectsViewComponent(HttpClient httpClient, UserManagementProxy userManagementProxy)
        {
            _httpClient = httpClient;
            _userManagementProxy = userManagementProxy;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManagementProxy.GetCurrentUserAsync();

            if (currentUser == null)
            {
                return View("Projects", new List<Project>());
            }

            var projectsToShow = await GetProjectsForUser(currentUser);

            return View("Projects", projectsToShow);
        }

        private async Task<List<Project>> GetProjectsForUser(ApplicationUser currentUser)
        {
            string apiUrl = await BuildApiUrlAsync(currentUser);
            return await _httpClient.GetFromJsonAsync<List<Project>>(apiUrl);
        }

        private async Task<string> BuildApiUrlAsync(ApplicationUser currentUser)
        {
            string role = await GetUserRole(currentUser);
            return $"http://localhost:5250/api/projects?userId={currentUser.Id}&role={role}";
        }

        private async Task<string> GetUserRole(ApplicationUser user)
        {
            if (await _userManagementProxy.IsUserInRoleAsync(user, "admin"))
            {
                return "admin";
            }
            else if (await _userManagementProxy.IsUserInRoleAsync(user, "manager"))
            {
                return "manager";
            }
            else
            {
                return "member";
            }
        }
    }
}