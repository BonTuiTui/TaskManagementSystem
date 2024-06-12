using Microsoft.AspNetCore.Authentication;
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
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);
        Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor);
        Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo info);
        Task SignInAsync(ApplicationUser user, bool isPersistent);
        Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string? token);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser? user);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);
        Task RefreshSignInAsync(ApplicationUser user);
    }
}