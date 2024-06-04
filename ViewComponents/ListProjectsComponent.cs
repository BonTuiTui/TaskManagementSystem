using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.ViewComponents
{
    [ViewComponent(Name = "Projects")]
    public class ProjectsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public ProjectsViewComponent(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("Projects", _context.Projects.ToList());
        }
    }
}