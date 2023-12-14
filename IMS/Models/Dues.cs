using System.ComponentModel.DataAnnotations;
namespace IMS.Models;

public class Dues
{
    [Key]
    public int Id { get; set; }
    public string? EmployeeId { get; set; }
    public string? Name { get; set; }
    public string? PolicyName { get; set; }
    public DateTime PolicyDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public string? DueDateFrom { get; set; }
    public string? DueDateTo { get; set; }
    public string? Status { get; set; }

}