using System.ComponentModel.DataAnnotations;

namespace IMS.Models;
public class PolicyRequests
{
    [Key]
    public int ID { get; set; }
    public string? EmployeeId { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? PolicyName { get; set; }
    public DateTime PolicyDate { get; set; }
    public decimal PolicyAmount { get; set; }
    public int PolicyDuration { get; set; }
    public string? Status { get; set; }

}