using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Proxies
{
    public class UserManagementProxy : IUserManagementService
    {
        private readonly IUserManagementService _realService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementProxy(IUserManagementService realService, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _realService = realService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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

        // Phương thức lấy thông tin người dùng hiện tại
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }

        // Phương thức kiểm tra người dùng có trong vai trò được chỉ định hay không
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
            // No admin check here since it's for regular users
            return await _realService.AddToRolesAsync(user, roles);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            EnsureAdmin();
            return await _realService.RemoveFromRolesAsync(user, roles);
        }

        internal Task<bool> IsUserInRoleAsync(string currentUser, string v)
        {
            throw new NotImplementedException();
        }

    }
}
