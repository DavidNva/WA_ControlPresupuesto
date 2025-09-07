using System.ComponentModel.DataAnnotations;
using WA_ControlPresupuesto.Validations;
namespace WA_ControlPresupuesto.Models
{
    public class Cuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(maximumLength: 50)]
        [PrimeraLetraMayusculas]
        public string Nombre { get; set; }

        [Display(Name = "Tipo de Cuenta")]
        public int TipoCuentaId { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength: 1000)]
        public string Descripcion { get; set; }


    }
}
