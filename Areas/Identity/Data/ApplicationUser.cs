using System;
using Microsoft.AspNetCore.Identity;

namespace TaskManagementSystem.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }

        [PersonalData]
        public DateTime CreateAt { get; set; }
    }
}