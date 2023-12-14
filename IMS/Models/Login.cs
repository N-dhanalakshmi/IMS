using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMS.Models
{
    public class Login
    {
    [Required(ErrorMessage = "UserName can't be empty")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).*$", ErrorMessage = "Provide strong password")]
    public string? Password { get; set; }
    }
}