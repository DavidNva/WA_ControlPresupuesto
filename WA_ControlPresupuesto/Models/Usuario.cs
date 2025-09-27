using System.ComponentModel.DataAnnotations;

namespace WA_ControlPresupuesto.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        public string EmailNormalizado { get; set; }

        [Required]
        public string Password { get; set; }
        public string PasswordHash { get; set; }

    }
}
