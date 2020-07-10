using System.ComponentModel.DataAnnotations;

namespace AuthServer.Controllers.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public override string ToString()
        {
            return "{ Username: " + Username + ", Password: " + Password + " }";
        }
    }
}