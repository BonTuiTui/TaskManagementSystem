using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Observer
{
    public class TaskObserver : IObserver
    {
        private readonly string _userId;
        private readonly ApplicationDbContext _context;

        public TaskObserver(string userId, ApplicationDbContext context)
        {
            _userId = userId;
            _context = context;
        }


        public async System.Threading.Tasks.Task Update(string message)
        {
            if (string.IsNullOrEmpty(_userId))
            {
                Console.WriteLine("Error: User ID is null or empty");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Error: Notification message is null or empty");
                return;
            }

            var notification = new Notification
            {
                User_id = _userId,
                Notification_text = message,
                CreateAt = DateTime.Now,
                IsRead = false
            };
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                Console.WriteLine("Notification saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving notification: {ex.Message}");
                // Log the exception or rethrow as needed
            }
        }

    }

}

