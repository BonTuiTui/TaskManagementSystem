using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;

public class NotificationViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string userId)
    {
        var unreadCount = await _context.Notifications
            .Where(n => n.User_id == userId && !n.IsRead)
            .CountAsync();

        return View(unreadCount);
    }
}