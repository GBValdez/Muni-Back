using System.ComponentModel.DataAnnotations;

namespace project.users.dto
{
    public class credentialsDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string password { get; set; } = null!;
    }
}