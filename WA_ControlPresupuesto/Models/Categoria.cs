using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WA_ControlPresupuesto.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, ErrorMessage = "El campo {0} no puede ser mayor a {1} caracteres")]
        public string Nombre { get; set; }

        [DisplayName("Tipo Operación")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
