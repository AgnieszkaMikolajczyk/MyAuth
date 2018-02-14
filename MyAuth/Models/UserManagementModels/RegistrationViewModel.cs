using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyAuth.Models.UserManagementModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Value is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Value is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [MinLength(3, ErrorMessage = "Password must have at least 3 signs")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [Compare("Password", ErrorMessage = "Passwords must be identical")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}