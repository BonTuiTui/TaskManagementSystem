using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Services
{
    // Interface định nghĩa các phương thức quản lý người dùng
    public interface IUserManagementService
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password); // Đăng ký người dùng mới
        Task<SignInResult> SignInUserAsync(string username, string password, bool rememberMe); // Đăng nhập người dùng
        Task SignOutUserAsync(); // Đăng xuất người dùng
        Task<ApplicationUser> GetUserAsync(string userId); // Lấy người dùng bằng ID
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user); // Cập nhật thông tin người dùng
        Task<IList<string>> GetAllRolesAsync(); // Lấy tất cả các vai trò
        Task<IList<string>> GetRolesAsync(ApplicationUser user); // Lấy các vai trò của người dùng
        Task<IList<ApplicationUser>> GetAllUsersAsync(); // Lấy tất cả người dùng
        Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles); // Thêm người dùng vào các vai trò
        Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles); // Xóa người dùng khỏi các vai trò
        Task<ApplicationUser> GetCurrentUserAsync(); // Lấy người dùng hiện tại
        Task<ApplicationUser> GetUserByEmailAsync(string email); // Lấy người dùng bằng email
        Task<ApplicationUser> GetUserByNameAsync(string userName); // Lấy người dùng bằng tên đăng nhập
        Task<ApplicationUser> GetUserByIdAsync(string userId); // Lấy người dùng bằng ID
    }
}