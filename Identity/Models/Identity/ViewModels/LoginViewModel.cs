using CustomIdentity.CustomStorageProvider.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models.Identity.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [ColumnDefinition(ColumnTitle = "UserEmail")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [ColumnDefinition(ColumnTitle = "PasswordHash")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
