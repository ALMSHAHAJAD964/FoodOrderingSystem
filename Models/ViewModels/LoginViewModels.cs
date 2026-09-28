using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models.ViewModels
{
    public class LoginViewModels
    {
        [Required]
        [StringLength(100)]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
