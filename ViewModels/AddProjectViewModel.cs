using System;
namespace TaskManagementSystem.ViewModels
{
	public class AddProjectViewModel
	{
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}

