using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"
                SELECT c.Id, c.Nombre, c.Balance, tc.Nombre AS TipoCuenta 
                FROM Cuentas c inner join TiposCuentas tc on tc.Id = c.TipoCuentaId
                WHERE tc.UsuarioId = @UsuarioId
                Order BY tc.Orden", new { usuarioId });
        }

        public async Task Crear(Cuenta cuenta)//Es como un void asincrono, porque no devuelve nada 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Cuentas (Nombre, TipoCuentaId, Balance, Descripcion)
                    VALUES (@Nombre, @TipoCuentaId, @Balance, @Descripcion);
                SELECT SCOPE_IDENTITY();", cuenta);//Usamos QuerySingleAsync<int> porque esperamos un unico resultado, que es el id que se acaba de insertar. Si no devolviera nada, usariamos ExecuteAsync.
            cuenta.Id = id;
        }

        
    }
}
