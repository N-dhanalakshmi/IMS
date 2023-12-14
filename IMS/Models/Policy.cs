using System.ComponentModel.DataAnnotations;
namespace IMS.Models;
public class Policy
{
    [Key]
    public int Id { get; set; }
    public string? EmployeeId { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public DateTime DateofBirth { get; set; }
    public string? PolicyName { get; set; }
    public DateTime PolicyDate { get; set; }
    public decimal PolicyAmount { get; set; }
    public int PolicyDuration { get; set; }
    public string? Status { get; set; } = "Pending";

}