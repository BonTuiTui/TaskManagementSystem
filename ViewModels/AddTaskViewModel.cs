using System;
namespace TaskManagementSystem.ViewModels
{
	public class AddTaskViewModel
	{
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; } // Allow null value
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public DateTime DueDate { get; set; }
    }
}

