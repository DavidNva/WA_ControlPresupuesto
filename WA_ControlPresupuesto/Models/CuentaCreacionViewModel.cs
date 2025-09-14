using Microsoft.AspNetCore.Mvc.Rendering;

namespace WA_ControlPresupuesto.Models
{
    public class CuentaCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem>? TiposCuentas { get; set; }//Clase especiales de ASP.NET CORE que nos permite crear listas desplegables en las vistas de Razor Pages o MVC Views de una manera muy sencilla
    }
}
