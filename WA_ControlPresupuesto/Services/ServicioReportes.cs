using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IServicioReportes
    {
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anio, dynamic ViegBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int anio, dynamic ViewBag);
    }

    public class ServicioReportes: IServicioReportes
    {
        private readonly IRepositorioTransacciones _repositorioTransacciones;
        private readonly HttpContext httpContext;
        public ServicioReportes(IRepositorioTransacciones repositorioTransacciones, IHttpContextAccessor httpContextAccessor)
        {
            _repositorioTransacciones = repositorioTransacciones;
            httpContext = httpContextAccessor.HttpContext;
        }//Inyectamos el repositorio de transacciones para poder usarlo en este servicio

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int anio, dynamic ViewBag)
        {
            //el dynamic Viewbag es para poder pasar informacion a la vista desde el servicio, es decir 
            //para poder usar Viewbag en el servicio y pasar informacion a la vista desde el servicio y no desde el controlador, aunque claro esta que este servicio será usado desde el controlador

            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var ObtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };//Es lo que vamos a mandar al repositorio para que nos devuelva las transacciones de esa cuenta en ese rango de fechas


            var transacciones = await _repositorioTransacciones.ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta);

            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            return modelo;
        }

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;//Ese -1 es para que me de el mes anterior, ejemplo hoy es 20/09/2025, entonces si le resto un mes, me da 20/08/2025, y de ahi le saco el mes, que es 8
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;//Ese -1 es para que me de el mes anterior, ejemplo hoy es 20/09/2025, entonces si le resto un mes, me da 20/08/2025, y de ahi le saco el anio, que es 2025

            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;//Ese 1 es para que me de el mes siguiente
            ViewBag.anioPosterior = fechaInicio.AddMonths(1).Year;//Ese 1 es para que me de el mes siguiente
            ViewBag.UrlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;//Con esto lo que estamos haciendo es obtener la url completa de la pagina actual, incluyendo los parametros de consulta, para poder usarla como url de retorno en la vista Detalle.cshtml
        }

        private static ReporteTransaccionesDetalladas GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                }).ToList();//Aqui lo que estamos haciendo es agrupar las transacciones por fecha, para que en la vista se muestren agrupadas por fecha, ejemplo: 01/01/2024, 02/01/2024, etc

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int anio)
        {
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
            return (fechaInicio, fechaFin);
        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anio, dynamic ViegBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var paremetro = new ParametroObtenerTransacionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await _repositorioTransacciones.ObtenerPorUsuarioId(paremetro);

            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViegBag, fechaInicio);
            return modelo;
        }
    }
}
