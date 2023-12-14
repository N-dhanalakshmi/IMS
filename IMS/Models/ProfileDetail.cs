using System.ComponentModel.DataAnnotations;
namespace IMS.Models
{
    public partial class ProfileDetail
    {
        [Required]
        public string? Name { get; set; }

        public string? Role { get; set; }

        public int Age { get; set; }
        [Required]
        public string? Phone { get; set; }
        public string? Address { get; set; }
        [Required(ErrorMessage = "You cant submit without Email Id/User Name")]
        public string? UserName { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        [Required]

        public string? EmployeeId { get; set; }
    }
}
