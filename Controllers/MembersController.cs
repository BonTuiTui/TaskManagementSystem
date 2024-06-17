using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Hubs;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string api_gateway = "http://localhost:5250/api/members";
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _dbContext;

        public MembersController(HttpClient httpClient, IHubContext<NotificationHub> hubContext, ApplicationDbContext dbContext) 
        {
            _httpClient = httpClient;
            _hubContext = hubContext;
            _dbContext = dbContext;
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
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == userName);
            var project = await _dbContext.Projects.SingleOrDefaultAsync(p => p.Project_id == projectId);

            if (user != null)
            {
                Console.WriteLine("HAHHA"+user);

            } else
            {
                Console.WriteLine("Failed");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
            var message = $"Bạn đã được thêm  project <a href='/Projects/Details/{project.Project_id}' target='_blank'>View Project</a> {project.Name}";
            var notification = new Notification
            {
                User_id = user.Id,
                Notification_text = message,
                CreateAt = DateTime.Now,
                IsRead = false
            };
            if (userName != null)
            {
                await _hubContext.Clients.User(user.Id).SendAsync("ReceiveNotification", message);
                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync();
                 
            } else
            {
                Console.WriteLine("Failed");
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

