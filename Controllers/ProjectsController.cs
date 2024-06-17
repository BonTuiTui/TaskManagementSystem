using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services.ProjectFactory;
using TaskManagementSystem.Services.Proxies;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManagementProxy _userManagementProxy;
        private readonly string api_gateway = "http://localhost:5250/api/projects";

        public ProjectsController(HttpClient httpClient, UserManagementProxy userManagementProxy) 
        {
            _httpClient = httpClient;
            _userManagementProxy = userManagementProxy;
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
            var response = await _httpClient.PostAsJsonAsync(api_gateway, viewModel);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var projectId = await response.Content.ReadFromJsonAsync<int>();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(Project receive_project)
        {
            var response = await _httpClient.PutAsJsonAsync(api_gateway, receive_project);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var projectId = await response.Content.ReadFromJsonAsync<int>();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{api_gateway}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        { 
            var response = await _httpClient.GetAsync($"{api_gateway}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var project = await response.Content.ReadFromJsonAsync<Project>();

            if (project == null)
            {
                return NotFound("Project not found");
            }

            var currentUser = await _userManagementProxy.GetCurrentUserAsync();
            var createdByUser = await _userManagementProxy.GetUserByIdAsync(project.User_id);

            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "admin"))
            {
                ViewBag.CreatedByUsername = createdByUser?.UserName;
                return View(project);
            }

            if (await _userManagementProxy.IsUserInRoleAsync(currentUser, "manager"))
            {
                if (project.User_id != currentUser.Id)
                {
                    return Forbid();
                }

                ViewBag.CreatedByUsername = createdByUser?.UserName;
                return View(project);
            }

            if (project.ProjectMembers.Any(pm => pm.UserId == currentUser.Id))
            {
                ViewBag.CreatedByUsername = createdByUser?.UserName;
                return View(project);
            }

            return Forbid();
        }
    }
}