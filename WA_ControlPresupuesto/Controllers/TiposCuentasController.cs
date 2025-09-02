using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
        }

        public IActionResult Crear()
        {
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {//Cuando decimos task, es como si dijeramos en el futuro va a retornar tal cosa, en este caso en el futuro va a retornar IActionResult
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = 1;

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }
            await repositorioTiposCuentas.Crear(tipoCuenta);

            return View(tipoCuenta);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = 1;//por lo pronto usaremos este hardcode

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }
        //Json es un formato para representar datos como una cadena de texto, sirve para llevar datos de un lugar a otro. Por ejemplo con json puedo tomar un objeto de javascript, serializarlo a json y trasmitirlo a una aplicacion de c#. Y tambien se puede hacer lo inverso, de la app de c#, serializarlo, llevarlo a JavaScript, deserealizarlo alli y acceder a sus valores

    }
}