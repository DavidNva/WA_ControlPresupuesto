using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuariosController(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if(!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario{Email = modelo.Email};
            var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

            if(!resultado.Succeeded)
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }

            return RedirectToAction("Index", "Transacciones");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
