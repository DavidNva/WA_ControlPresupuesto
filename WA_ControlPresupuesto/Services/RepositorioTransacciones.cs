using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string _connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>("sp_Transacciones_Insertar", 
                new { 
                    transaccion.UsuarioId, 
                    transaccion.FechaTransaccion, 
                    transaccion.Monto, 
                    transaccion.CategoriaId, 
                    transaccion.CuentaId, 
                    transaccion.Nota 
                },commandType:System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }
    }
}
