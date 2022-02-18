using CustomIdentity.CustomStorageProvider.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models.Identity.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        [ColumnDefinition(ColumnTitle = "UserEmail")]
        public string Email { get; set; }
    }
}
