using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Proxies
{
    public class UserManagementProxy : IUserManagementService
    {
        private readonly IUserManagementService _realService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementProxy(IUserManagementService realService, IHttpContextAccessor httpContextAccessor)
        {
            _realService = realService;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsAdmin()
        {
            var user = _httpContextAccessor.HttpContext.User;
            return user.IsInRole("admin");
        }

        private void EnsureAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Only admins can perform this action.");
            }
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
            EnsureAdmin();
            return await _realService.AddToRolesAsync(user, roles);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            EnsureAdmin();
            return await _realService.RemoveFromRolesAsync(user, roles);
        }
    }
}
