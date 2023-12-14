using System.ComponentModel.DataAnnotations;
namespace IMS.Models;
public class ChangePassword
{

    [Required(ErrorMessage = "Username is required")]

    [DataType(DataType.EmailAddress)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).*$", ErrorMessage = "Provide valid password")]
    public string? OldPassword { get; set; }
    [Required(ErrorMessage = "This field is required")]
    [RegularExpression("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).*$", ErrorMessage = "Provide strong password")]
    public string? NewPassword { get; set; }

}
