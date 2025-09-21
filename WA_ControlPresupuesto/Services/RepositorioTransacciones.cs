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
    }
}
