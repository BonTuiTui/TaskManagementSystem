using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TaskCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action to load comments of a specific task
        [HttpGet]
        public async Task<IActionResult> GetTaskComments(int taskId)
        {
            var taskComments = await _context.TaskComments
                .Where(c => c.Task_id == taskId)
                .Select(c => new
                {
                    c.Comment_text,
                    c.CreateAt,
                    UserName = c.User.UserName // Assuming UserName is the field in ApplicationUser
                })
                .ToListAsync();

            return Json(taskComments);
        }

        // Action to add a new comment to a task
        [HttpPost]
        public async Task<IActionResult> AddTaskComment(TaskComment taskComment)
        {
            if (ModelState.IsValid)
            {
                taskComment.CreateAt = DateTime.UtcNow;
                _context.TaskComments.Add(taskComment);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
