using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WA_ControlPresupuesto.Models
{
    public class TransaccionCreacionViewModel:Transaccion
    {
        public IEnumerable<SelectListItem> Cuentas { get; set; }
        public IEnumerable<SelectListItem> Categorias { get; set; }

        [Display(Name = "Tipo de Operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
    }
}
