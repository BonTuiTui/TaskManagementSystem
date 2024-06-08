using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManagementSystem.Proxies
{
    public class UserManagementProxy : IUserManagementService
    {
        private readonly IUserManagementService _realService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserManagementProxy(IUserManagementService realService, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _realService = realService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser> GetUserByNameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        private bool IsAdmin()
        {
            var user = _httpContextAccessor.HttpContext.User;
            return user.IsInRole("admin");
        }

        private bool IsManager()
        {
            var user = _httpContextAccessor.HttpContext.User;
            return user.IsInRole("manager");
        }

        private void EnsureAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Only admins can perform this action.");
            }
        }

        private void EnsureManager()
        {
            if (!IsManager())
            {
                throw new UnauthorizedAccessException("Only manager can perform this action.");
            }
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password)
        {
            return await _realService.RegisterUserAsync(user, password);
        }

        public async Task<SignInResult> SignInUserAsync(string username, string password, bool rememberMe)
        {
            return await _realService.SignInUserAsync(username, password, rememberMe);
        }

        public async Task SignOutUserAsync()
        {
            await _realService.SignOutUserAsync();
        }

        public async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await _realService.GetUserAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _realService.UpdateUserAsync(user);
        }

        public async Task<IList<string>> GetAllRolesAsync()
        {
            EnsureAdmin();
            return await _realService.GetAllRolesAsync();
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            EnsureAdmin();
            return await _realService.GetRolesAsync(user);
        }

        public async Task<IList<ApplicationUser>> GetAllUsersAsync()
        {
            EnsureAdmin();
            return await _realService.GetAllUsersAsync();
        }

        public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            return await _realService.AddToRolesAsync(user, roles);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            EnsureAdmin();
            return await _realService.RemoveFromRolesAsync(user, roles);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        // New methods for external login support
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
    }
}
