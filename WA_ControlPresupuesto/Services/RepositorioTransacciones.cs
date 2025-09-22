using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaIdAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anio);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransacionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransacionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string _connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT t.*, c.TipoOperacionId 
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                WHERE t.Id = @Id AND t.UsuarioId = @UsuarioId",
                new { id, usuarioId });
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>("sp_Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                }, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("sp_Transacciones_Actualizar",
               new
               {
                   transaccion.Id,
                   transaccion.FechaTransaccion,
                   transaccion.Monto,
                   transaccion.CategoriaId,
                   transaccion.CuentaId,
                   transaccion.Nota,
                   montoAnterior,
                   cuentaAnteriorId
               }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("sp_Transacciones_Eliminar", new { id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        #region recuperar transacciones
        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                SELECT t.Id,t.Monto, t.FechaTransaccion, c.Nombre as Categoria, 
                cu.Nombre as Cuenta, c.TipoOperacionId 
                FROM Transacciones t 
                INNER JOIN Categorias c ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu ON cu.Id = t.CuentaId
                WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin 
                ORDER BY t.FechaTransaccion DESC", modelo);
            //Modelo ya tiene  las propiedades que necesita la consulta, por eso se puede mandar asi directamente
        }
        #endregion

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransacionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                SELECT t.Id,t.Monto, t.FechaTransaccion, c.Nombre as Categoria, 
                cu.Nombre as Cuenta, c.TipoOperacionId 
                FROM Transacciones t 
                INNER JOIN Categorias c ON c.Id = t.CategoriaId
                INNER JOIN Cuentas cu ON cu.Id = t.CuentaId
                WHERE t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin 
                ORDER BY t.FechaTransaccion DESC", modelo);//Con Dapper no es necesario abrir y cerrar la conexion, lo hace automaticamente
        }
    
        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransacionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                SELECT DATEDIFF(D,@fechaInicio, t.FechaTransaccion) / 7+1 as Semana,
                SUM(t.Monto) AS Monto, c.TipoOperacionId
                FROM Transacciones t 
                INNER JOIN Categorias c 
                ON c.Id = t.CategoriaId
                WHERE t.UsuarioId = @usuarioId AND t.FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                GROUP BY DATEDIFF(D,@fechaInicio, FechaTransaccion)/7, c.TipoOperacionId", modelo);
        }


        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anio)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
                SELECT MONTH(t.FechaTransaccion) as Mes, 
                SUM(t.Monto) as Monto, c.TipoOperacionId
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.Id = t.CategoriaId
                WHERE t.UsuarioId = @usuarioId AND YEAR(t.FechaTransaccion) = @Anio
                GROUP BY MONTH(t.FechaTransaccion), c.TipoOperacionId
                ORDER BY MONTH(t.FechaTransaccion) DESC", new { usuarioId, anio });
        }




        //Porque algunos metodos son async y otros no?
        //Esto ocurreo porque los metodos que realizan operaciones de E/S (Entrada/Salida) como acceder a bases de datos o servicios web son inherentemente asincronos. 
        //Al marcar estos metodos como async, permitimos que el hilo que llama pueda continuar ejecutandose mientras espera la respuesta de la operacion de E/S, mejorando la capacidad de respuesta y escalabilidad de la aplicacion.
        //Ahora, porque algunos son asyn Task IEnumerable<T> y otros son async Task<T>? o simplemente  public async Task Crear
        //La diferencia radica en el tipo de resultado que devuelve el metodo:
        //1. Task<T>: Se utiliza cuando el metodo devuelve un unico valor de tipo T. Por ejemplo, en ObtenerPorId, el metodo devuelve un unico objeto Transaccion.
        //2. Task<IEnumerable<T>>: Se utiliza cuando el metodo devuelve una coleccion de valores de tipo T. Por ejemplo, en ObtenerPorCuentaId, el metodo devuelve una coleccion de objetos Transaccion.

        //Los metodos de crear, editar o eliminar por ejemplo no devuelven ningun valor, por eso son simplemente async Task (es como si fueran void pero async)
    }
}
