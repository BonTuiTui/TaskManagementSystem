using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.ViewModels;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TaskManagementSystem.Interfaces;
using Microsoft.AspNet.Identity;
using TaskManagementSystem.Services.Proxies;

namespace TaskManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManagementProxy _userManagementProxy;
        private readonly ISenderEmail _emailSender;
        public AccountController(UserManagementProxy userManagementProxy, ISenderEmail emailSender)
        {
            _userManagementProxy = userManagementProxy;
            _emailSender = emailSender;
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
                var existingEmail = await _userManagementProxy.GetUserByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
                    return View(model);
                }

                // Tách tên người dùng từ địa chỉ email
                var userName = model.Email.Split('@')[0];
                var fullName = model.Email.Split('@')[0]; // Giả sử fullname là phần trước @ của email

                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = model.Email,
                    FullName = fullName
                };

                var result = await _userManagementProxy.RegisterUserAsync(user);

                if (result.Succeeded)
                {
                    await SendConfirmationEmail(model.Email, user);
                    await _userManagementProxy.AddToRolesAsync(user, new List<string> { "employee" });
                    await _userManagementProxy.SignInUserAsync(user.UserName, model.Password, rememberMe: false);
                    return View("RegistrationSuccessful");
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
                var user = await _userManagementProxy.GetUserByNameAsync(model.Username);

                if (user != null && await _userManagementProxy.IsUserInRoleAsync(user, "employee") && user.LockoutEnd > DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been blocked by the admin.");
                    return View(model);
                }

                var result = await _userManagementProxy.SignInUserAsync(model.Username, model.Password, model.RememberMe);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    return RedirectToAction("Lockout");

                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            model.ExternalLogins = (await _userManagementProxy.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(model);
        }


        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            var token = await _userManagementProxy.GenerateEmailConfirmationTokenAsync(user);
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Confirm Your Email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                ViewBag.Message = "The link is Invalid or Expired";
            }
            //Find the User By Id
            var user = await _userManagementProxy.GetUserByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {UserId} is Invalid";
                return View("NotFound");
            }
            //Call the ConfirmEmailAsync Method which will mark the Email as Confirmed
            var result = await _userManagementProxy.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                ViewBag.Message = "Thank you for confirming your email";
                return View();
            }
            ViewBag.Message = "Email cannot be confirmed";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await _userManagementProxy.GetUserByEmailAsync(Email);
            if (user == null || await _userManagementProxy.IsEmailConfirmedAsync(user))
            {
                // Handle the situation when the user does not exist or Email already confirmed.
                // For security, don't reveal that the user does not exist or Email is already confirmed
                return View("ConfirmationEmailSent");
            }

            //Then send the Confirmation Email to the User
            await SendConfirmationEmail(Email, user);

            return View("ConfirmationEmailSent");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        private async Task SendForgotPasswordEmail(string? email, ApplicationUser? user)
        {
            var token = await _userManagementProxy.GeneratePasswordResetTokenAsync(user);
            var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { Email = email, Token = token }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Reset Your Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(passwordResetLink)}'>clicking here</a>.", true);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManagementProxy.GetUserByEmailAsync(model.Email);

                if (user != null)
                    {
                    await SendForgotPasswordEmail(user.Email, user);
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
                ModelState.AddModelError(string.Empty, "Email not found.");
                return View(model);
            }

            return View(model);
        }
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Token, string Email)
        {
            if (Token == null || Email == null)
            {
                ViewBag.ErrorTitle = "Invalid Password Reset Token";
                ViewBag.ErrorMessage = "The Link is Expired or Invalid";
                return View("Error");
            }
            else
            {
                ResetPasswordViewModel model = new ResetPasswordViewModel();
                model.Token = Token;
                model.Email = Email;
                return View(model);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManagementProxy.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManagementProxy.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManagementProxy.GetUserAsync(User.Identity.GetUserId());
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _userManagementProxy.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _userManagementProxy.RefreshSignInAsync(user);

                return RedirectToAction("ChangePasswordConfirmation", "Account");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userManagementProxy.SignOutUserAsync();
            return RedirectToAction("Index", "Home");
        }
        // Change Password for Google , Facebook ......
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await _userManagementProxy.GetUserAsync(userId: User.Identity.GetUserId());
            var userHasPassword = await _userManagementProxy.HasPasswordAsync(user);
            if (userHasPassword)
            {
                return RedirectToAction("ChangePasswordForGoogle", "Account");
            }
            return View();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePasswordForGoogle()
        {
            var user = await _userManagementProxy.GetUserAsync(userId: User.Identity.GetUserId());
            var userHasPassword = await _userManagementProxy.HasPasswordAsync(user);
            if (!userHasPassword)
            {
                return RedirectToAction("AddPassword", "Account");
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManagementProxy.GetUserAsync(userId: User.Identity.GetUserId());
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to Load User.");
                    return View();
                }

                var result = await _userManagementProxy.AddPasswordAsync(user, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _userManagementProxy.RefreshSignInAsync(user);

                return RedirectToAction("AddPasswordConfirmation", "Account");
            }

            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult AddPasswordConfirmation()
        {
            return View();
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
                    if (user != null && user.LockoutEnd > DateTime.Now)
                    {
                        await _userManagementProxy.SignOutAsync();
                        return RedirectToAction("Lockout");
                    }
                    if (user == null)
                    {
                        Console.WriteLine("User not found, creating new user");

                        var userName = email.Split('@')[0];
                        Console.WriteLine($"Username: {userName}");

                        user = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            FullName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        };
                        var createResult = await _userManagementProxy.RegisterUserAsync(user);
                        if (!createResult.Succeeded)
                        {
                            foreach (var error in createResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View("Login", loginViewModel);
                        }
                    


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
        //Lockout User for Admin
        public IActionResult Lockout()
        {
            return View();
        }
    }
}
