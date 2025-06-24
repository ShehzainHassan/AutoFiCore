using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class ContactInfo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string SelectedOption { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string VehicleName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PostCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PreferredContactMethod { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Comment { get; set; }

        [Required]
        public bool EmailMeNewResults { get; set; }
    }
}
