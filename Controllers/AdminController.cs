using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.ViewModels;
using TaskManagementSystem.Services.Proxies;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace TaskManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManagementProxy _userManagementProxy;

        public AdminController(UserManagementProxy userManagementProxy)
        {
            _userManagementProxy = userManagementProxy;
        }

        public async Task<IActionResult> ManageUsersAsync(string searchTerm)
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
                    Comfirmed = user.EmailConfirmed,
                    Roles = userRoles.ToList(),
                    LockoutEnd = user.LockoutEnd
                };
                userViewModels.Add(userViewModel);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                userViewModels = userViewModels
                  .Where(u => u.UserName.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm))
                  .ToList();
            }

            ViewBag.SearchTerm = searchTerm;

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
                Roles = allRoles.ToList(),
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId)
        {
            var user = await _userManagementProxy.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UsersViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return View("LockUserConfirmation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUserConfirmed(string userId)
        {
            var user = await _userManagementProxy.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManagementProxy.LockUserAsync(userId);
            if (result.Succeeded)
            {
                TempData["Message"] = "User has been successfully locked.";
            }
            else
            {
                TempData["Error"] = "An error occurred while locking the user.";
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManagementProxy.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManagementProxy.UnlockUserAsync(userId);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User has been successfully unlocked.";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while unlocking the user.";
            }

            return RedirectToAction("ManageUsers");
        }
    }
}
