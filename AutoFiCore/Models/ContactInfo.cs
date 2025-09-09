using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents contact details submitted by a user, typically during a vehicle inquiry or auction interest.
    /// </summary>
    public class ContactInfo
    {
        /// <summary>
        /// Unique identifier for the contact record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// First name of the user.
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "First name cannot exceed 20 characters.")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the user.
        /// </summary>
        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Selected option or interest type (e.g., interested in car, wants to test drive etc).
        /// </summary>
        [Required]
        [StringLength(30, ErrorMessage = "Selected option cannot exceed 30 characters.")]
        public string SelectedOption { get; set; } = string.Empty;

        /// <summary>
        /// Name or model of the vehicle the user is interested in.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters.")]
        public string VehicleName { get; set; } = string.Empty;

        /// <summary>
        /// Postal code of the user's location.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Post code cannot exceed 50 characters.")]
        public string PostCode { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the user.
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Preferred method of contact (e.g., Email, Phone).
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "Preferred contact method cannot exceed 20 characters.")]
        public string PreferredContactMethod { get; set; } = string.Empty;

        /// <summary>
        /// Optional comment or message from the user.
        /// </summary>
        [StringLength(100, ErrorMessage = "Comment cannot exceed 100 characters.")]
        public string? Comment { get; set; }

        /// <summary>
        /// Indicates whether the user wants to receive email updates about new results.
        /// </summary>
        [Required]
        public bool EmailMeNewResults { get; set; }
    }
}