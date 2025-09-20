using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas _repositorioTiposCuentas;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly IMapper _mapper;//Por convencion el name deberia ser _mapper, porque es un campo privado , pero en los ejemplos de automapper lo ponen asi, sin el _.
        private readonly IRepositorioTransacciones _repositorioTransacciones;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IMapper mapper, IRepositorioTransacciones repositorioTransacciones)
        {
            _repositorioTiposCuentas = repositorioTiposCuentas;
            _servicioUsuarios = servicioUsuarios;
            _repositorioCuentas = repositorioCuentas;
            _mapper = mapper;//No necesitamos poner el this porque no hay ambiguedad entre el campo y el parametro del constructor
            _repositorioTransacciones = repositorioTransacciones;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await _repositorioCuentas.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta.
                GroupBy(x => x.TipoCuenta)//Vamos a agrupar las cuentas por el TipoCuenta, por ejemplo: Ahorros, Tarjetas, etc
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,//Es simplemente igual a TipoCuenta porque estamos agrupando por TipoCuenta
                    Cuentas = grupo.AsEnumerable()//Esto es para convertir el grupo en una enumeracion de cuentas.  Es decir, por ejempllo, que si el grupo es Ahorros, entonces Cuentas sera una enumeracion de todas las cuentas que pertenecen a Ahorros
                }).ToList();
            //En si este metodo dice: Agrupame las cuentas por TipoCuenta, y por cada grupo crea un nuevo IndiceCuentasViewModel, donde el TipoCuenta es la clave del grupo (el nombre del TipoCuenta) y las Cuentas son todas las cuentas que pertenecen a ese grupo, por ejemplo: 
            // Grupo 1: TipoCuenta = "Ahorros", Cuentas = [ { Nombre = "Cuenta Ahorros Banco1", Balance = 1500 }, { Nombre = "Cuenta Ahorros Banco2", Balance = 2500 }, { Nombre = "Cuenta Ahorros Banco3", Balance = 3000 } ]  
            // Grupo 2: TipoCuenta = "Tarjetas", Cuentas = [ { Nombre = "Tarjeta Visa", Balance = -500 }, { Nombre = "Tarjeta MasterCard", Balance = -200 } ]

            //Como balance es una propiedad calculada, no necesitamos hacer nada especial para que se calcule, ya que al acceder a la propiedad Balance, automaticamente se ejecuta el codigo que suma los balances de las cuentas
            //En si, retornamos una lista de IndiceCuentasViewModel, donde cada IndiceCuentasViewModel representa un grupo de cuentas del mismo TipoCuenta y dentro de cada IndiceCuentasViewModel tenemos la lista de cuentas que pertenecen a ese TipoCuenta 


            return View(modelo);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            //El primer valor es el texto y el segundo es el valor. En este caso el nombre es el display y el valor a pasar es el id
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {//Si el modelo no es valido, volvemos a cargar los tipos de cuentas para que el usuario pueda ver el dropdown
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await _repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await _repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        public async Task<IActionResult> Editar(int id)//Solo hace la consulta de  TiposCuentas para llenar el dropdown y lo retornar a la vista y demas datos para el autollenado de cuando se este editando
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = _mapper.Map<CuentaCreacionViewModel>(cuenta);//Ya con esto estoy mapeando los datos de cuenta a CuentaCreacionViewModel
            //Explicado es decir antes teniamos: 
            //var modelo = new CuentaCreacionViewModel();
            //modelo.Id = cuenta.Id;
            //modelo.Nombre = cuenta.Nombre;
            //modelo.Balance = cuenta.Balance;
            //modelo.Descripcion = cuenta.Descripcion;
            //modelo.TipoCuentaId = cuenta.TipoCuentaId;
            //Ahora con el mapper, ya no es necesario hacer  eso, porque el automapper lo hace por nosotros. Solo debemos asegurarnos que las propiedades tengan el mismo nombre en ambos modelos.
            //Y como funciona esto? Porque en Startup.cs o Program.cs, donde sea que estemos configurando los servicios, ya hemos configurado el automapper y le hemos dicho que mapee entre Cuenta y CuentaCreacionViewModel. Entonces, cuando llamamos a mapper.Map<CuentaCreacionViewModel>(cuenta), el automapper sabe que debe tomar las propiedades de cuenta y asignarlas a un nuevo objeto de tipo CuentaCreacionViewModel.
            //Es un poco dificil de entender al principio, pero es muy util cuando tenemos muchos modelos y queremos evitar escribir mucho codigo repetitivo para mapear entre ellos.
            //Un ejemplo mas claro es si tenemos un modelo de entidad que representa una tabla en la base de datos y un modelo de vista que representa los datos que queremos mostrar en la vista. Entonces, podemos usar el automapper para mapear entre estos dos modelos sin tener que escribir mucho codigo repetitivo.
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var tipoCuenta = await _repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            if (!ModelState.IsValid)
            {
                cuentaEditar.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuentaEditar);
            }
            await _repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await _repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
    
        public async Task<IActionResult> Detalle(int id, int mes, int anio)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            DateTime fechaInicio;
            DateTime fechaFin;
            if (mes <= 0 || mes > 12 || anio <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                //Es decir, lo que estamos haciendo es obtener el primer dia del mes actual
            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);//Primer dia del mes y anio especificado
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);//Ultimo dia del mes especificado

            var ObtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = id,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };//Es lo que vamos a mandar al repositorio para que nos devuelva las transacciones de esa cuenta en ese rango de fechas
            var transacciones = await _repositorioTransacciones.ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta);
            var modelo = new ReporteTransaccionesDetalladas();
            ViewBag.Cuenta = cuenta.Nombre;
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo=> new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                }).ToList();//Aqui lo que estamos haciendo es agrupar las transacciones por fecha, para que en la vista se muestren agrupadas por fecha, ejemplo: 01/01/2024, 02/01/2024, etc

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;//Ese -1 es para que me de el mes anterior, ejemplo hoy es 20/09/2025, entonces si le resto un mes, me da 20/08/2025, y de ahi le saco el mes, que es 8
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;//Ese -1 es para que me de el mes anterior, ejemplo hoy es 20/09/2025, entonces si le resto un mes, me da 20/08/2025, y de ahi le saco el anio, que es 2025

            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;//Ese 1 es para que me de el mes siguiente
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;//Ese 1 es para que me de el mes siguiente

            return View(modelo);
        }
    }
}
