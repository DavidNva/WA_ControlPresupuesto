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
        public string Nombre { get; set; }//En este caso si indicamos que es obligatorio, por lo que no puede ser null, ni cadena vacia. Aparece un tipo de advertencia en el IDE, pero no es un error. Lo podemos quitar: #nullable disable. La ventja de ponerlo asi es que nos avisa si por error dejamos que pueda ser null. La desventaja es que no nos avisa si dejamos que pueda ser cadena vacia.Entonces, mejor poner [Required] y quitar el #nullable disable.
        //Si no queremos ver esa advertencia, podemos poner #nullable disable al principio del archivo. Pero es mejor dejarlo asi para que nos avise si por error dejamos que pueda ser null.

        [Display(Name = "Tipo de Cuenta")]
        public int TipoCuentaId { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength: 1000)]
        public string? Descripcion { get; set; }//Por default, valida en teoria nulls o empty strings, cadenas vacias, es decir, aunque no pusimos explicitamente [Required], si lo dejabamos asi, en la vista nos lo pediria obligatoriamente. Por ello, si queremos que sea opcional, debemos poner el ? despues del tipo de dato. 
        //Ese signo ? indica que puede ser null esa propiedad, campo,etc. 


    }
}
