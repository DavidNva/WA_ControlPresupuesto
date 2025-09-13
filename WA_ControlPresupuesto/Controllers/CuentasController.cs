using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WA_ControlPresupuesto.Models;
using WA_ControlPresupuesto.Services;

namespace WA_ControlPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, IMapper mapper)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

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
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            //El primer valor es el texto y el segundo es el valor. En este caso el nombre es el display y el valor a pasar es el id
            return View(modelo);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {//Si el modelo no es valido, volvemos a cargar los tipos de cuentas para que el usuario pueda ver el dropdown
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Editar(int id)//Solo hace la consulta de  TiposCuentas para llenar el dropdown y lo retornar a la vista y demas datos para el autollenado de cuando se este editando
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);//Ya con esto estoy mapeando los datos de cuenta a CuentaCreacionViewModel
            //Explicado es decir antes teniamos: 
            //var modelo = new CuentaCreacionViewModel();
            //modelo.Id = cuenta.Id;
            //modelo.Nombre = cuenta.Nombre;
            //modelo.Balance = cuenta.Balance;
            //modelo.Descripcion = cuenta.Descripcion;
            //modelo.TipoCuentaId = cuenta.TipoCuentaId;
            //Ahora con el mapper, ya no es necesario hacer todo eso, porque el automapper lo hace por nosotros. Solo debemos asegurarnos que las propiedades tengan el mismo nombre en ambos modelos.
            //Y como funciona esto? Porque en Startup.cs o Program.cs, donde sea que estemos configurando los servicios, ya hemos configurado el automapper y le hemos dicho que mapee entre Cuenta y CuentaCreacionViewModel. Entonces, cuando llamamos a mapper.Map<CuentaCreacionViewModel>(cuenta), el automapper sabe que debe tomar las propiedades de cuenta y asignarlas a un nuevo objeto de tipo CuentaCreacionViewModel.
            //Es un poco dificil de entender al principio, pero es muy util cuando tenemos muchos modelos y queremos evitar escribir mucho codigo repetitivo para mapear entre ellos.
            //Un ejemplo mas claro es si tenemos un modelo de entidad que representa una tabla en la base de datos y un modelo de vista que representa los datos que queremos mostrar en la vista. Entonces, podemos usar el automapper para mapear entre estos dos modelos sin tener que escribir mucho codigo repetitivo.
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            if (!ModelState.IsValid)
            {
                cuentaEditar.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuentaEditar);
            }
            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");

        }
    }
}
