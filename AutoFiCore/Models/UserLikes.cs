using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class UserLikes
    {
        public int userId { get; set; }
        [Required]
        [StringLength(17, MinimumLength = 17)]
        public string vehicleVin { get; set; } = string.Empty;

    }
}
