using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Proxies
{
    // Proxy để quản lý người dùng, đảm bảo quyền truy cập và ủy quyền các hành động cho dịch vụ thực sự
    // Lớp proxy để quản lý người dùng, đảm bảo quyền truy cập và ủy quyền các hành động cho dịch vụ thực sự
    public class UserManagementProxy : IUserManagementService
    {
        private readonly IUserManagementService _realService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Khởi tạo proxy với dịch vụ thực sự và HttpContextAccessor
        public UserManagementProxy(IUserManagementService realService, IHttpContextAccessor httpContextAccessor)
        {
            _realService = realService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Kiểm tra nếu người dùng hiện tại là admin
        private bool IsAdmin()
        {
            var user = _httpContextAccessor.HttpContext.User;
            return user.IsInRole("admin");
        }

        // Kiểm tra nếu người dùng hiện tại là manager
        private bool IsManager()
        {
            var user = _httpContextAccessor.HttpContext.User;
            return user.IsInRole("manager");
        }

        // Đảm bảo người dùng là admin, ném ngoại lệ nếu không phải
        private void EnsureAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Only admins can perform this action.");
            }
        }

        // Đảm bảo người dùng là manager, ném ngoại lệ nếu không phải
        private void EnsureManager()
        {
            if (!IsManager())
            {
                throw new UnauthorizedAccessException("Only manager can perform this action.");
            }
        }

        // Đăng ký người dùng mới
        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password)
        {
            return await _realService.RegisterUserAsync(user, password);
        }

        // Đăng nhập người dùng
        public async Task<SignInResult> SignInUserAsync(string username, string password, bool rememberMe)
        {
            return await _realService.SignInUserAsync(username, password, rememberMe);
        }

        // Đăng xuất người dùng
        public async Task SignOutUserAsync()
        {
            await _realService.SignOutUserAsync();
        }

        // Lấy người dùng bằng ID
        public async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await _realService.GetUserAsync(userId);
        }

        // Cập nhật thông tin người dùng
        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _realService.UpdateUserAsync(user);
        }

        // Lấy tất cả các vai trò, chỉ admin mới có quyền
        public async Task<IList<string>> GetAllRolesAsync()
        {
            EnsureAdmin();
            return await _realService.GetAllRolesAsync();
        }

        // Lấy các vai trò của người dùng, chỉ admin mới có quyền
        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            EnsureAdmin();
            return await _realService.GetRolesAsync(user);
        }

        // Lấy tất cả người dùng, chỉ admin mới có quyền
        public async Task<IList<ApplicationUser>> GetAllUsersAsync()
        {
            EnsureAdmin();
            return await _realService.GetAllUsersAsync();
        }

        // Thêm người dùng vào các vai trò
        public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            return await _realService.AddToRolesAsync(user, roles);
        }

        // Xóa người dùng khỏi các vai trò, chỉ admin mới có quyền
        public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            EnsureAdmin();
            return await _realService.RemoveFromRolesAsync(user, roles);
        }

        // Lấy người dùng hiện tại
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _realService.GetCurrentUserAsync();
        }

        // Kiểm tra nếu người dùng thuộc vai trò nhất định
        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _realService.IsUserInRoleAsync(user, roleName);
        }

        // Lấy người dùng bằng email
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _realService.GetUserByEmailAsync(email);
        }

        // Lấy người dùng bằng tên đăng nhập
        public async Task<ApplicationUser> GetUserByNameAsync(string username)
        {
            return await _realService.GetUserByNameAsync(username);
        }

        // Lấy người dùng bằng ID
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _realService.GetUserByIdAsync(userId);
        }

        // Các phương thức hỗ trợ đăng nhập từ bên ngoài (External login)
        
        // Lấy các schema xác thực từ bên ngoài
        public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            return await _realService.GetExternalAuthenticationSchemesAsync();
        }

        // Cấu hình thuộc tính xác thực từ bên ngoài
        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _realService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        // Lấy thông tin đăng nhập từ bên ngoài
        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _realService.GetExternalLoginInfoAsync();
        }

        // Đăng nhập bằng thông tin từ bên ngoài
        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            return await _realService.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }

        // Thêm thông tin đăng nhập từ bên ngoài cho người dùng
        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo info)
        {
            return await _realService.AddLoginAsync(user, info);
        }

        // Đăng nhập người dùng với tùy chọn isPersistent
        public async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            await _realService.SignInAsync(user, isPersistent);
        }
    }
}