using AutoFiCore.Dto;
using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class Questionnaire
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DrivingLicense { get; set; } = string.Empty;

        [Required]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required]
        public DateOnly DOB { get; set; }

        [Required]
        public string EmploymentStatus { get; set; } = string.Empty;

        public decimal BorrowAmount { get; set; }

        public Boolean? NotSure { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;
    }
}
