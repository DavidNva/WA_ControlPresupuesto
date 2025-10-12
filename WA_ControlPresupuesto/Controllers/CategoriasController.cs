using Microsoft.AspNetCore.Mvc;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias _repositorioCategorias;
        private readonly IServicioUsuarios _servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            _repositorioCategorias = repositorioCategorias;
            _servicioUsuarios = servicioUsuarios;
        }
        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacionViewModel)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categorias = await _repositorioCategorias.Obtener(usuarioId, paginacionViewModel);

            var totalCategorias = await _repositorioCategorias.Contar(usuarioId);
            var respuestaViewModel = new PaginacionRespuesta<Categoria>
            {
                Elementos = categorias,
                Pagina = paginacionViewModel.Pagina,
                RecordsPorPagina = paginacionViewModel.RecordsPorPagina,
                CantidadTotalRecords = totalCategorias,
                //BaseUrl = "/categorias"//Obtiene la URL del endpoint actual, es decir /Categorias/Index
                BaseUrl = Url.Action()//Obtiene la URL del endpoint actual, es decir /Categorias/Index
                //Url helpers son métodos de extensión que nos permiten generar URLs para acciones, rutas, contenido estático, etc. Url.Action() genera una URL para una acción específica en un controlador.
            };

            return View(respuestaViewModel);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)//Primero validamos el modelo segun las reglas de data annotations que tenga, como required, stringlength, etc.
            {
                return View(categoria);
            }
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            categoria.UsuarioId = usuarioId;
            await _repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)//Esto es lo que aparece al entrar a la vista de editar, que es el formulario con los datos de la categoria
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categoriaDB = await _repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoriaDB is null)//Significa que la categoría no existe o no pertenece al usuario logueado.
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoriaDB);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)//Esto es lo que aparece al hacer click en el boton de editar, que es el formulario con los datos de la categoria
        {
            if (!ModelState.IsValid)//Primero validamos el modelo segun las reglas de data annotations que tenga, como required, stringlength, etc.
            {
                return View(categoriaEditar);
            }
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categoriaDB = await _repositorioCategorias.ObtenerPorId(categoriaEditar.Id, usuarioId);
            if (categoriaDB is null)//Significa que la categoría no existe o no pertenece al usuario logueado.
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            categoriaEditar.UsuarioId = usuarioId;//Si pertenece, asignamos el usuarioId al objeto categoria que viene del formulario.
            await _repositorioCategorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categoriaDB = await _repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoriaDB is null)//Significa que la categoría no existe o no pertenece al usuario logueado.
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoriaDB);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var categoriaDB = await _repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoriaDB is null)//Significa que la categoría no existe o no pertenece al usuario logueado.
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await _repositorioCategorias.Borrar(id);
            return RedirectToAction("Index");
        }
      
    }
}
