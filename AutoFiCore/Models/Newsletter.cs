using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents a newsletter subscription entry containing a user's email address.
    /// </summary>
    public class Newsletter
    {
        /// <summary>
        /// Unique identifier for the newsletter subscription record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The email address of the user subscribing to the newsletter.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(25, ErrorMessage = "Email cannot exceed 25 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
}