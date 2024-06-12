using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
