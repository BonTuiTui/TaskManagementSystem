using System.Text.Json;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string api_gateway = "http://localhost:5250/api/notifications";

        public NotificationsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationsCount(string userId)
        {
            var response = await _httpClient.GetAsync($"{api_gateway}/unreadcount/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var count = await response.Content.ReadFromJsonAsync<int>();

            return Json(count);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var response = await _httpClient.GetAsync($"{api_gateway}/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var notifications = await response.Content.ReadFromJsonAsync<dynamic>();

            return Ok(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            
            var response = await _httpClient.PostAsync($"{api_gateway}/{notificationId}", null);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return Ok();
        }
    }
}