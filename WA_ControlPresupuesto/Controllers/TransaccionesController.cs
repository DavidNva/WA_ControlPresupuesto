using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IRepositorioTransacciones _repositorioTransacciones;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly IRepositorioCategorias _repositorioCategorias;
        private readonly IServicioUsuarios _servicioUsuarios;
        public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IRepositorioCategorias repositorioCategorias )
        {
            _servicioUsuarios = servicioUsuarios;
            _repositorioCuentas = repositorioCuentas;
            _repositorioCategorias = repositorioCategorias;
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await _repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(cuenta => new SelectListItem(cuenta.Nombre, cuenta.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await _repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(categoria => new SelectListItem(categoria.Nombre, categoria.Id.ToString()));
        }//Hacemos primero esto, porque lo vamos a necesitar en el metodo Crear y en el metodo ObtenerCategorias. Lo separamos en un metodo aparte para no repetir codigo y porque en el metodo Crear no necesitamos devolver un IActionResult, sino solo un IEnumerable<SelectListItem>, ademas de que en el método de listar. Lo hacemos tipo IENumerable<SelectListItem> porque es lo que necesita el select en la vista. para mostrar usando asp-items

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }//Este metodo es llamado desde el archivo crear.cshtml, en el script que esta al final del archivo, cuando se cambia el valor del select de tipo de operacion
         //Creamos a parte el metodo ObtenerCategorias para no repetir codigo, ya que lo necesitamos en el metodo Crear y en este metodo
    }
}
