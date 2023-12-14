using System.ComponentModel.DataAnnotations;

namespace IMS.Models;
public class ClaimNow
{
    [Key]
    public int Id { get; set; }
    [Display(Name = "Employee Id")]
    public string? EmployeeId { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    [Display(Name = "Date of Birth")]
    public DateTime DateofBirth { get; set; }
    [Display(Name = "Policy Name")]
    public string? PolicyName { get; set; }
    [Display(Name = "Policy Date")]
    public DateTime PolicyDate { get; set; }
    [Display(Name = "Policy Amount")]
    public decimal? PolicyAmount { get; set; }
    [Display(Name = "Policy Duration")]
    public int PolicyDuration { get; set; }
    public DateTime ClaimRequestDate { get; set; } = DateTime.Now;
    [Required(ErrorMessage = "Give valid reason to claim policy before the due")]
    public string? Reason { get; set; }
    [Required(ErrorMessage = "Select the proof document")]
    [Display(Name = "Medical Proof(pdf)")]
    public byte[]? MedicalProof { get; set; }
    [Required(ErrorMessage = "Add bill receipt to make sure your request")]
    [Display(Name = "Bill Receipt")]
    public byte[]? Receipt { get; set; }

}


