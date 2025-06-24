using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class UserSavedSearch
    {
        public int userId { get; set; }

        [Required]
        public string search { get; set; } = string.Empty;
    }
}
