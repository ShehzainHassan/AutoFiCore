using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Dto
{
    public class QuestionnaireDTO
    {
        [Required(ErrorMessage = "Driving License is required.")]
        public string DrivingLicense { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marital Status is required.")]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "DOB is required.")]
        public DateOnly DOB { get; set; }

        [Required(ErrorMessage = "Employment Status is required.")]
        public string EmploymentStatus { get; set; } = string.Empty;

        public decimal BorrowAmount { get; set; }

        public Boolean? NotSure { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string Phone { get; set; } = string.Empty;
    }
}
