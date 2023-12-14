using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models;
public class Response
{
    [Key]
    public int idkey { get; set; }
    public string? Username { get; set; }
    public string? EmployeeId { get; set; }
    public string? Reply { get; set; }
    public DateTime ReplyDate { get; set; } = DateTime.Now;
    public int id { get; set; }
    [ForeignKey("id")]
    public Help? help { get; set; }
}