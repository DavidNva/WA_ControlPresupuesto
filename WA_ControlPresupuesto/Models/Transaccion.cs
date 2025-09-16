using System.ComponentModel.DataAnnotations;

namespace WA_ControlPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [DataType(DataType.DateTime)]
        //public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:MM tt"));
        public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("g"));//podemos usar estas letras, las vemos en el intelligent code al poner ToString y podemos usarlas para dar formato a la fecha, ejemplo "g" es fecha y hora corta, "d" es solo fecha corta, "t" es solo hora corta, etc., https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        public decimal Monto { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]

        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [StringLength(1000, ErrorMessage = "La nota no puede tener más de {1} caracteres")]
        public string Nota { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }
    }
}
