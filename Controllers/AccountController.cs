using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Proxies;
using TaskManagementSystem.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

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
        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel
            {
                ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnUrl = Url.Action("Index", "Home")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUsername = await _userManagementProxy.GetUserByNameAsync(model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
                    return View(model);
                }

                var existingEmail = await _userManagementProxy.GetUserByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.Fullname
                };

                var result = await _userManagementProxy.RegisterUserAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManagementProxy.AddToRolesAsync(user, new List<string> { "employee" });
                    await _userManagementProxy.SignInUserAsync(user.UserName, model.Password, rememberMe: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? ReturnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = ReturnUrl,
                ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManagementProxy.SignInUserAsync(model.Username, model.Password, model.RememberMe);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }
            model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userManagementProxy.SignOutUserAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action(action: "ExternalLoginCallback", controller: "Account", values: new { ReturnUrl = returnUrl });
            var properties = _userManagementProxy.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            Console.WriteLine("ExternalLoginCallback called");
            returnUrl = returnUrl ?? Url.Content("~/");
            Console.WriteLine($"ReturnUrl: {returnUrl}");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            Console.WriteLine("LoginViewModel created");

            if (remoteError != null)
            {
                Console.WriteLine($"Remote error: {remoteError}");
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            var info = await _userManagementProxy.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Console.WriteLine("ExternalLoginInfo is null");
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login", loginViewModel);
            }
            Console.WriteLine("ExternalLoginInfo retrieved");

            var signInResult = await _userManagementProxy.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            Console.WriteLine($"SignInResult: {signInResult.Succeeded}");

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                Console.WriteLine($"Email: {email}");

                if (email != null)
                {
                    var user = await _userManagementProxy.GetUserByEmailAsync(email);
                    if (user == null)
                    {
                        Console.WriteLine("User not found, creating new user");
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            FullName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        };
                        var createResult = await _userManagementProxy.RegisterUserAsync(user, "P@ssword123"); // Thay thế "defaultPassword" bằng mật khẩu phù hợp
                        Console.WriteLine($"Create user result: {createResult.Succeeded}");

                        if (!createResult.Succeeded)
                        {
                            foreach (var error in createResult.Errors)
                            {
                                Console.WriteLine($"Create user error: {error.Description}");
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View("Login", loginViewModel);
                        }

                        // Thêm vai trò mặc định "employee" cho người dùng mới
                        var addToRoleResult = await _userManagementProxy.AddToRolesAsync(user, new List<string> { "employee" });
                        Console.WriteLine($"Add to role result: {addToRoleResult.Succeeded}");

                        if (!addToRoleResult.Succeeded)
                        {
                            foreach (var error in addToRoleResult.Errors)
                            {
                                Console.WriteLine($"Add to role error: {error.Description}");
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View("Login", loginViewModel);
                        }
                    }
                    await _userManagementProxy.AddLoginAsync(user, info);
                    await _userManagementProxy.SignInAsync(user, isPersistent: false);
                    Console.WriteLine("User signed in");
                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support.";
                Console.WriteLine("Email claim not received");
                return View("Error");
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManagementProxy.GetUserByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
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

            var user = await _userManagementProxy.GetUserByIdAsync(model.UserId);
            if (user == null)
            {
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
