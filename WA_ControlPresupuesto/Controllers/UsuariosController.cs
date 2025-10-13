using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IServicioEmail _servicioEmail;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IServicioEmail servicioEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _servicioEmail = servicioEmail;
        }

        [AllowAnonymous]//Permite que usuarios no autenticados puedan acceder a este metodo
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvideMiPassword(string mensaje = "")
        {
            ViewBag.Mensaje = mensaje;
            //El view bag es una forma de pasar informacion de un controlador a una vista. 
            //El view bag se puede pasar de controller a vista y de vista a vista, pero no de vista a controller
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> OlvideMiPassword(OlvideMiPasswordViewModel modelo)
        {
            var mensaje = "Proceso concluido. Si el email dado se corresponde con uno de nuestros usuarios, en su bandeja de entrada podrá encontrar las instrucciones para recuperar su contraseña.";
            ViewBag.Mensaje = mensaje;
            ModelState.Clear();
           
            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if(usuario is null)
            {
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var tokenBase64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var enlace = Url.Action("RecuperarPassword", "Usuarios", new { token = tokenBase64 }, protocol: Request.Scheme);
            ////Enviar el email
            await _servicioEmail.EnviarEmailCambioPassword(modelo.Email, enlace);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword(string token)
        {
            if (token is null)
            {
                var mensaje = "Token no ecnontrado";
                return RedirectToAction("OlvideMiPassword", new { mensaje});
            }

            var modelo = new RecuperarPasswordViewModel();
            modelo.TokenReseteo = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            return View(modelo);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task <IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo)
        {
            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if(usuario is null)
            {
                return RedirectToAction("PasswordCambiado");
            }
            var resultado = await _userManager.ResetPasswordAsync(usuario, modelo.TokenReseteo, modelo.Password);
            return RedirectToAction("PasswordCambiado");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordCambiado()
        {
            return View();
        }
    }
}