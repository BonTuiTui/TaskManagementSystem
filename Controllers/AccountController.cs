using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNet.Identity;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManagementProxy _userManagementProxy;

        public AccountController(UserManagementProxy userManagementProxy)
        {
            _userManagementProxy = userManagementProxy;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the Username is already taken
                var existingUsername = await _userManagementProxy.GetUserByNameAsync(model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(model);
                }

                // Check if the Email is already taken
                var existingEmail = await _userManagementProxy.GetUserByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    return View(model);
                }

                var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.Fullname, CreateAt = model.CreateAt };
                var result = await _userManagementProxy.RegisterUserAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the "employee" role to the new user
                    await _userManagementProxy.AddToRolesAsync(user, new List<string> { "employee" });

                    await _userManagementProxy.SignInUserAsync(user.UserName, model.Password, rememberMe: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManagementProxy.SignInUserAsync(model.Username, model.Password, model.RememberMe);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userManagementProxy.SignOutUserAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManagementProxy.GetUserAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManagementProxy.GetUserAsync(model.UserId);
            if (user == null)
            {
                Console.WriteLine("NTBDuy here ... ... ... NOT FOUND");
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;

            var result = await _userManagementProxy.UpdateUserAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}


