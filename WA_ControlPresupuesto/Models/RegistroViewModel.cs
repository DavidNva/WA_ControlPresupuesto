using System.ComponentModel.DataAnnotations;

namespace WA_ControlPresupuesto.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un correo electrónico válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }  
    }
}
