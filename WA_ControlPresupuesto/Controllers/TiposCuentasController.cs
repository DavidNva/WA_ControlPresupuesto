using Microsoft.AspNetCore.Mvc;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
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
            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }
            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }


        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }
        //Json es un formato para representar datos como una cadena de texto, sirve para llevar datos de un lugar a otro. Por ejemplo con json puedo tomar un objeto de javascript, serializarlo a json y trasmitirlo a una aplicacion de c#. Y tambien se puede hacer lo inverso, de la app de c#, serializarlo, llevarlo a JavaScript, deserealizarlo alli y acceder a sus valores


        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
                //No encontrado es la vista que hicimos en HomeController y el Home es el controlador
            }
            return View(tipoCuenta);
        }//Este metodo es usado para mostrar la vista de confirmacion de borrado, aunque parezca que no hace nada, si hace, porque busca el tipo de cuenta a borrar y si no lo encuentra redirige a la vista de no encontrado

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
                //No encontrado es la vista que hicimos en HomeController y el Home es el controlador
            }
            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }//Este metodo es usado para hacer el borrado en si, es decir cuando se confirma el borrado


        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);

            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);//Obtenemos los ids de los tipos cuentas. Lo podemos leer de la siguiente forma para no confundirnos con los x
            //Decimos "toma los tiposCuentas y por cada uno de esos tiposCuentas (a los que llamamos x) dame su Id" 

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();//Verificar que los ids sean del usuario //De manera sencilla y entendible, decimos "toma los ids que me enviaste y quítales los ids que pertenecen al usuario, si queda alguno es porque no pertenece al usuario"
            //Es decir, nosotros tenemos un array de ids que nos envian desde el front, y tenemos los ids de los tipos de cuentas que pertenecen al usuario, entonces le decimos "toma los ids que me enviaste y quítales los ids que pertenecen al usuario, si queda alguno es porque no pertenece al usuario"
            //Es como si compararamos, si tenemos [1,2,3,4] y [1,2], al hacer el except quedaria [3,4], que son los que no pertenecen al usuario
            //Un ejemplo completo para ya entender este flujo desde el inicio de la funcion es el siguiente: //Supongamos que el usuario tiene los tipos de cuentas con ids [1,2,3] y desde el front nos envian los ids [2,3,4], entonces al hacer el except quedaria [4], que es el que no pertenece al usuario, por lo tanto no deberia poder ordenar ese id 4 porque no es suyo, por eso retornamos un Forbid
            //Ahora en el caso correcto, por ejemplo, el flujo seria: //Supongamos que el usuario tiene los tipos de cuentas con ids [1,2,3] y desde el front nos envian los ids [2,3,1], entonces al hacer el except quedaria [], es decir, no quedaria ninguno, porque todos los ids que nos enviaron pertenecen al usuario, por lo tanto si deberia poder ordenar esos ids. En teoria siempre debe pasar esto, porque el front deberia enviar siempre los ids correctos, pero si alguien hace una peticion maliciosa, entonces si podria enviar ids que no son del usuario, por eso hacemos esta validacion. 

            //Entonces, explicado para niños: //Tenemos los ids que el usuario tiene (1,2,3) y los ids que el usuario nos envio para ordenar (2,3,4), entonces le decimos "toma los ids que me enviaste (2,3,4) y quítales los ids que pertenecen al usuario (1,2,3), si queda alguno es porque no pertenece al usuario", en este caso quedaria (4), que es el que no pertenece al usuario, por lo tanto no deberia poder ordenar ese id 4 porque no es suyo, por eso retornamos un Forbid 

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();//403 //Significa que no tiene permisos, prohibido
            }
            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
            new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            //explicado de manera sencilla y simple de entender, decimos: toma los ids que me enviaste y por cada uno de esos ids dame su valor y su indice, con eso crea un nuevo objeto de tipo TipoCuenta, asignale el id con el valor que me diste y el orden con el indice + 1 (porque el indice empieza en 0) y al final convierte todo eso en una enumeracion

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        }
    }
}