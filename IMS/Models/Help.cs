using System.ComponentModel.DataAnnotations;
namespace IMS.Models;
public class Help
{
    [Key]
    public int id { get; set; }
    public string? Username { get; set; }
    public string? EmployeeId { get; set; }
    public string? Request { get; set; }
    public DateTime SentDate { get; set; } = DateTime.Now;

}
