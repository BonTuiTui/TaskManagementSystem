using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }
    }
}
