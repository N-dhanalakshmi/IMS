using System.ComponentModel.DataAnnotations;
namespace IMS.Models;
public class LoginDetails
{

    public string EmployeeId { get; set; }
    public string Name { get; set; }

    [Required(ErrorMessage = "Username is required")]

    [DataType(DataType.EmailAddress)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).*$", ErrorMessage = "Provide strong password")]
    public string? Password { get; set; }

}
