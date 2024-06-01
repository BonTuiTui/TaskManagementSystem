using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.ViewModels
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Roles")]
        public List<string> Roles { get; set; }

        [Display(Name = "User Roles")]
        public List<string> UserRoles { get; set; }
    }
}