using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Users
{
    public class RegisterRequest
    {
        [Required]
        public string Name{ get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
