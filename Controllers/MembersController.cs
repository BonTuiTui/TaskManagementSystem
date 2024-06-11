using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string api_gateway = "http://localhost:5250/api/members";

        public MembersController(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            var response = await _httpClient.GetAsync($"{api_gateway}/{projectId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var projectMembers = await response.Content.ReadFromJsonAsync<dynamic>();

            return Ok(projectMembers);
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> AddUserToProject(int projectId, string userName)
        {
            var response = await _httpClient.PostAsync($"{api_gateway}?projectId={projectId}&userName={userName}", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var data = await response.Content.ReadFromJsonAsync<dynamic>();

            return Ok(data);
        }



        [HttpDelete]
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemoveUserFromProject(int projectId, string userId)
        {
            var response = await _httpClient.DeleteAsync($"{api_gateway}?projectId={projectId}&userId={userId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var data = await response.Content.ReadFromJsonAsync<dynamic>();

            return Ok(data);
        }
    }
}

