using Microsoft.AspNetCore.Mvc;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            return View(tipoCuenta);
        }

    }
}