using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Proxies;

namespace TaskManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManagementProxy _userManagementProxy;

    // Khởi tạo HomeController với UserManagementProxy thông qua Dependency Injection
    public HomeController(UserManagementProxy userManagementProxy)
    {
        _userManagementProxy = userManagementProxy;
    }

    // Phương thức Index hiển thị thông tin người dùng hiện tại
    public async Task<IActionResult> Index()
    {
        // Lấy thông tin người dùng hiện tại
        var user = await _userManagementProxy.GetCurrentUserAsync();
        return View(user);
    }

    // Phương thức xử lý lỗi và chuyển hướng đến trang NotFound cho lỗi 404
    [AllowAnonymous]
    public IActionResult Error(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound");
        }

        // Xử lý các mã trạng thái khác nếu cần thiết
        return View("Error");
    }
}