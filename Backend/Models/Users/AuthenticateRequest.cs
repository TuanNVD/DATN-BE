using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Users
{
    public class AuthenticateRequest
    {
        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}
