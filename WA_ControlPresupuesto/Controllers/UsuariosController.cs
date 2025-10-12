using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario { Email = modelo.Email };
            var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }
            await _signInManager.SignInAsync(usuario, isPersistent: true);//Esto sirve para
            return RedirectToAction("Index", "Transacciones");
        }

        [HttpGet]
        public IActionResult Login()
        {
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = await _signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);//Si el user colocara varias veces mal su password y queremos que se bloquee la cuenta, debemos cambiar a  true en lockoutOnFailure
            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }

            ModelState.AddModelError(string.Empty, "Nombre de Usuario o Password incorrecto");
            return View(modelo);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transacciones");
        }
    }
}