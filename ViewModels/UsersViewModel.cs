﻿using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.ViewModels
{
    public class UsersViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        public Boolean Comfirmed { get; set; }

        [Display(Name = "Roles")]
        public IList<string> Roles { get; set; }
        public bool? LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }


    }
}