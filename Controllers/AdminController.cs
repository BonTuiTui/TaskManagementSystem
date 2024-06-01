using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.ViewModels;
using TaskManagementSystem.Proxies;

namespace TaskManagementSystem.Controllers;

public class AdminController : Controller
{
    private readonly UserManagementProxy _userManagementProxy;

    public AdminController(UserManagementProxy userManagementProxy)
    {
        _userManagementProxy = userManagementProxy;
    }

    public async Task<IActionResult> ManageUsersAsync()
    {
        var users = await _userManagementProxy.GetAllUsersAsync();
        var userViewModels = new List<UsersViewModel>();

        foreach (var user in users)
        {
            var userRoles = await _userManagementProxy.GetRolesAsync(user);
            var userViewModel = new UsersViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = userRoles.ToList()
            };
            userViewModels.Add(userViewModel);
        }

        return View(userViewModels);
    }

    public async Task<IActionResult> EditRoles(string userId)
    {
        var user = await _userManagementProxy.GetUserAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManagementProxy.GetRolesAsync(user);
        var allRoles = await _userManagementProxy.GetAllRolesAsync();

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Roles = (List<string>)allRoles,
            UserRoles = userRoles.ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
    {
        var user = await _userManagementProxy.GetUserAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManagementProxy.GetRolesAsync(user);
        var selectedRoles = model.UserRoles ?? new List<string>();

        var result = await _userManagementProxy.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded)
        {
            // Handle error
        }

        result = await _userManagementProxy.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            // Handle error
        }

        return RedirectToAction("ManageUsers");
    }
}
