﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Interfaces;

namespace TaskManagementSystem.Services.Proxies
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task<IdentityResult> RegisterUserAsync1(ApplicationUser user, string password)
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>(); // Use the built-in hasher
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            return await _userManager.CreateAsync(user);
        }
        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task<SignInResult> SignInUserAsync(string username, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: false);
        }

        public async Task SignOutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IList<string>> GetAllRolesAsync()
        {
            return await Task.FromResult(_roleManager.Roles.Select(r => r.Name).ToList());
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IList<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            return await _userManager.AddToRolesAsync(user, roles);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser> GetUserByNameAsync(string userName)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            return await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }

        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo info)
        {
            return await _userManager.AddLoginAsync(user, info);
        }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }

        public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password)
        {
            try
            {
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> ValidatePasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> LockUserAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UnlockUserAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.LockoutEnd = null;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string? token)
        {
            if (user == null || string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("User or token cannot be null or empty.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result;
        }

        public async Task RefreshSignInAsync(ApplicationUser user)
        {
            await _signInManager.RefreshSignInAsync(user);
        }
        public async Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return await _userManager.HasPasswordAsync(user);
        }

        public async Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string newPassword)
        {
            var result = await _userManager.AddPasswordAsync(user, newPassword);
            return result;
        }
    }
}
