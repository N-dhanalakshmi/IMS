namespace IMS.Models
{
    public class RegistrationModel
    {
        public string? Name { get; set; }
        public string? EmployeeId { get; set; }
        public string? PolicyName { get; set; }
        public DateTime PolicyDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
    }
}