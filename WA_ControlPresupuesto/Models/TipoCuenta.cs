using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using WA_ControlPresupuesto.Validations;

namespace WA_ControlPresupuesto.Models
{
    public class TipoCuenta/*: IValidatableObject*/
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name ="Nombre del tipo cuenta")]
        [PrimeraLetraMayusculas]
        [Remote(action: "VerificarExisteTipoCuenta", controller:"TiposCuentas")]//Esto nos sirve para hacer una validacion remota, es decir, que se haga una llamada al servidor para validar si el valor ya existe en la base de datos, esto sin necesidad de hacer un postback, es decir, sin necesidad de recargar la pagina
        //funcion antes de guardar, se hace una llamada ajax al servidor para validar si el valor ya existe en la base de datos, entonces el usuario no necesita espera a pulsar el boton de guardar para saber si el valor ya existe, desde que lo pone, le diremos si ya existe o no
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden {  get; set; }



        //public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
        //{
        //    if(Nombre !=null && Nombre.Length > 0)
        //    {
        //        var primeraLetra = Nombre[0].ToString();
        //        //if(primeraLetra != primeraLetra.ToUpper())
        //        //{//Esto tambien se puede hacer y asi validamos por atrubuto desde aqui en la misma clase, claro aunnque ya no seria global para demas models
        //        //    yield return new ValidationResult("La primera letra debe ser mayúscula desde ValidateModel", 
        //        //        new[] {nameof(Nombre)});
        //        //}

        //        if (primeraLetra != primeraLetra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayúscula desde ValidateModel");
        //        }//de esta forma esta validacion no aplica a un solo atributo,sino al modelo
        //    }
        //}
        /*Pruebas de otras validaciones por defecto*/

        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1}")]
        //[Display(Name = "Tipo cuenta")]
        //public string Tipo { get; set; }


        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[EmailAddress(ErrorMessage ="El campo debe ser un correo electrónico válido")]
        //public string Email { get; set; }

        //[Range(minimum:18, maximum:130, ErrorMessage ="El valor debe estar entre {1} y {2}")]
        //public int Edad {  get; set; }

        //[Url(ErrorMessage ="El campo debe ser una URL válida")]
        //public string URL { get; set; }

        //[CreditCard(ErrorMessage ="La tarjeta de crédito no es válida")]
        //[Display(Name = "Tarjeta de Crédito")]
        //public string TarjetaDeCredito {  get; set; }




    }
}