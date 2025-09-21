using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IRepositorioTransacciones _repositorioTransacciones;
        private readonly IMapper _mapper;
        private readonly IServicioReportes servicioReportes;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly IRepositorioCategorias _repositorioCategorias;
        private readonly IServicioUsuarios _servicioUsuarios;
        public TransaccionesController(IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IRepositorioCategorias repositorioCategorias, IRepositorioTransacciones repositorioTransacciones, IMapper mapper, IServicioReportes servicioReportes)
        {
            _servicioUsuarios = servicioUsuarios;
            _repositorioCuentas = repositorioCuentas;
            _repositorioCategorias = repositorioCategorias;
            _repositorioTransacciones = repositorioTransacciones;
            _mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index(int mes, int anio)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, anio, ViewBag);
            return View(modelo);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await _repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await _repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)//Con esto estamos validando que la categoria que se esta intentando usar para crear la transaccion, realmente exista y pertenezca al usuario que esta intentando crear la transaccion.
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto = modelo.Monto * -1;//Lo hacemos asi porque el monto en la base de datos se guarda como negativo para los gastos y positivo para los ingresos.
            }
            await _repositorioTransacciones.Crear(modelo);

            return RedirectToAction("Index");
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

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await _repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = _mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;//Si es un ingreso
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;//Lo hacemos asi porque el monto en la base de datos se guarda como negativo para los gastos y positivo para los ingresos.
            }
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);

        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {//Si el modelo no es valido, volvemos a cargar las cuentas y categorias para que el usuario pueda corregir los errores
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await _repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await _repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var transaccion = _mapper.Map<Transaccion>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto = modelo.Monto * -1;//Lo hacemos asi porque el monto en la base de datos se guarda como negativo para los gastos y positivo para los ingresos.
            }
            await _repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            if(string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");//Si la url de retorno es nula o vacia, redirigimos al index de transacciones, es decir estamos actualizando desde el index de transacciones
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);//Con LocalRedirect nos aseguramos que la url de retorno sea una url local y no una url externa, para evitar ataques de redireccionamiento abierto.
            }
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await _repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {//Validamos que la transaccion que se esta intentando borrar, realmente exista y pertenezca al usuario que esta intentando borrarla.
                return RedirectToAction("NoEncontrado", "Home");
            }
            await _repositorioTransacciones.Borrar(id);
           if(string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");//Si la url de retorno es nula o vacia, redirigimos al index de transacciones, es decir estamos borrando desde el index de transacciones
            }
            else
            {
                return LocalRedirect(urlRetorno);//Con LocalRedirect nos aseguramos que la url de retorno sea una url local y no una url externa, para evitar ataques de redireccionamiento abierto.
            }

            //segun lo que hemos hecho, tanto para editar, borrar y el de cancelar con el link reference en la vista, al commit de github lo podemos llamar de la siguiente forma: "Implementación de edición, borrado y retorno seguro en TransaccionesController" con su descripcion: "Se ha añadido la funcionalidad para editar y borrar transacciones, así como manejar retornos seguros a URLs locales para mejorar la navegación del usuario." 
        }
    }
}
