
using System.ComponentModel.DataAnnotations;

namespace project.users.dto
{
    public class userCreationDto : credentialsDto
    {
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [StringLength(100, ErrorMessage = "El campo Nombre no puede tener más de 100 caracteres")]
        public string name { get; set; } = null!;

        [Required(ErrorMessage = "El campo Fecha de Nacimiento es requerido")]
        public DateOnly birthdate { get; set; }

        [Required(ErrorMessage = "El campo Teléfono es requerido")]
        [StringLength(10, ErrorMessage = "El campo Teléfono no puede tener más de 10 caracteres")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "El campo Dirección es requerido")]
        [StringLength(100, ErrorMessage = "El campo Dirección no puede tener más de 100 caracteres")]
        public string address { get; set; } = null!;
        [Required(ErrorMessage = "El campo DPI es requerido")]
        [StringLength(8, ErrorMessage = "El campo DPI no puede tener más de 8 caracteres")]
        public string dpi { get; set; }
    }
}