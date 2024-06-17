using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Services.Proxies;

namespace TaskManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManagementProxy _userManagementProxy;

    public HomeController(UserManagementProxy userManagementProxy)
    {
        _userManagementProxy = userManagementProxy;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManagementProxy.GetCurrentUserAsync();
        return View(user);
    }

    [AllowAnonymous]
    public IActionResult Error(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound");
        }
        return View("Error");
    }
}