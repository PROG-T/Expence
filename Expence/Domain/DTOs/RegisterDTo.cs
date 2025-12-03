using System.ComponentModel.DataAnnotations;

namespace Expence.Domain.DTOs
{
    public class RegisterDTo
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
