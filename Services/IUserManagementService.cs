using Microsoft.AspNetCore.Identity;
using TaskManagementSystem.Areas.Identity.Data;

namespace TaskManagementSystem.Services
{
    public interface IUserManagementService
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password);
        Task<SignInResult> SignInUserAsync(string username, string password, bool rememberMe);
        Task SignOutUserAsync();
        Task<ApplicationUser> GetUserAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IList<string>> GetAllRolesAsync();
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<IList<ApplicationUser>> GetAllUsersAsync();
        Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetUserByNameAsync(string userName);

    }
}
