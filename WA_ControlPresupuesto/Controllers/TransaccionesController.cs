using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Semanal(int mes, int anio)
        {
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesporSemana = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, anio, ViewBag);

            var agrupado = transaccionesporSemana.GroupBy(x => x.Semana).Select(g => new ResultadoObtenerPorSemana
            {
                Semana = g.Key,//Puede ser 1,2,3,4 o 5 dependiendo del mes
                Ingresos = g.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).
                Select(x=>x.Monto).FirstOrDefault(),//De esta forma tenemos el ingreso total de la semana
                Gastos = g.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).
                Select(x=>x.Monto).FirstOrDefault(),//Tenemos el gasto total de la semana
            }).ToList();//Con esto estamos agrupando las transacciones por semana y sumando los ingresos y gastos de cada semana

            if(anio == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);//Con esto obtenemos todos los dias del mes, por ejemplo, si es enero, obtenemos del 1 al 31, pero si fuese febrero, obtenemos del 1 al 28 o 29 dependiendo si es bisiesto o no

            var diasSegmentados = diasDelMes.Chunk(7).ToList();//Con esto estamos dividiendo los dias del mes en semanas, es decir, si el mes tiene 31 dias, obtenemos 5 arrays, los primeros 4 con 7 dias y el ultimo con 3 dias

            for(int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i + 1;//La semana puede ser 1,2,3,4 o 5 dependiendo del mes
                var fechaInicio = new DateTime(anio, mes, diasSegmentados[i].First());//Obtenemos la fecha de inicio de la semana
                var fechaFin = new DateTime(anio, mes, diasSegmentados[i].Last());//Obtenemos la fecha de fin de la semana

                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {//Si ya existe el grupo de la semana, solo actualizamos la fecha de inicio y fin
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x=>x.Semana).ToList();//Ordenamos las semanas de mayor a menor, es decir, la semana 5 primero y la semana 1 al final

            var modelo = new ReportesSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;
            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int anio)
        {//Poruqe es Task IActionResult y no solo IActionResult? Porque vamos a hacer una llamada asincrona
            //es decir , vamos a llamar a un metodo que devuelve una tarea, en este caso, ObtenerPorMes, que es un metodo asincrono y devuelve una tarea que contiene una lista de ResultadoObtenerPorMes
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
            if (anio == 0)
            {
                var hoy = DateTime.Today;
                anio = hoy.Year;
            }
            var transaccionesporMes = await _repositorioTransacciones.ObtenerPorMes(usuarioId, anio);
            var transaccionesAgrupadas = transaccionesporMes.GroupBy(x=> x.Mes)
                .Select(x => new ResultadoObtenerPorMes
                {
                    Mes = x.Key,
                    Ingreso = x.Where(y => y.TipoOperacionId == TipoOperacion.Ingreso)
                                .Select(y => y.Monto).FirstOrDefault(),
                    Gasto = x.Where(y => y.TipoOperacionId == TipoOperacion.Gasto)
                                .Select(y => y.Monto).FirstOrDefault()
                }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var referencia = new DateTime(anio, mes, 1);
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes
                    {
                        Mes = mes,
                        FechaReferencia = referencia
                    });//Si no existe la transaccion, la agregamos con el mes y la fecha de referencia para que en la vista se muestre el mes
                }
                else
                {
                    transaccion.FechaReferencia = referencia;//Si ya existe la transaccion, solo actualizamos la fecha de referencia
                }
            }
            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            var modelo = new ReporteMensualViewModel();
            modelo.Anio = anio;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;
            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);//Esto significa el primer dia del mes indicado en el parametro mes y anio
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);//El ultimo dia del mes
            var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

            var transacciones = await _repositorioTransacciones.ObtenerPorUsuarioId(
                new ParametroObtenerTransacionesPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"ManejoPresupuesto_{fechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            //Usamos la libreria ClosedXML para generar el excel
            //usaremos un datatable, que es como una representacion de una tabla en memoria
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("FechaTransaccion"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });//Definimos las columnas de la tabla
            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId == TipoOperacion.Ingreso ? "Ingreso" : "Gasto"
                );
            }//Agregamos las filas a la tabla
            using (XLWorkbook libro = new XLWorkbook())
            {
                libro.Worksheets.Add(dataTable);//Las hojas del excel. Agregamos la tabla al libro
                using (MemoryStream stream = new MemoryStream())
                {//Lo que estamos haciendo es crear un archivo en memoria, sin necesidad de guardarlo en el disco duro
                    libro.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);//el to array es porque el metodo File necesita un arreglo de bytes
                }//Devolevmos el archivo en memoria como un arreglo de bytes, con el tipo de contenido y el nombre del archivo
            }
        }

        public IActionResult Calendario()
        {
            return View();
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
