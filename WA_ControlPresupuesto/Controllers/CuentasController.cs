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

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            var modelo =  cuentasConTipoCuenta.
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

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if(!ModelState.IsValid)
            {//Si el modelo no es valido, volvemos a cargar los tipos de cuentas para que el usuario pueda ver el dropdown
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
