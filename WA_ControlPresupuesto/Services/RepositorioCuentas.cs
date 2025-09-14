using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(Cuenta cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
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

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)//Hacemos el inner join de Cuentas y TiposCuentas para asegurarnos que la cuenta por medio de su id que estamos obteniendo pertenece al usuario que hizo la peticion.
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
            @"SELECT c.Id, c.Nombre, c.Balance, c.Descripcion, c.TipoCuentaId
            FROM Cuentas c INNER JOIN TiposCuentas tc ON tc.Id = c.TipoCuentaId 
            WHERE c.Id = @Id AND tc.UsuarioId = @UsuarioId", new { id, usuarioId });
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


        public async Task Actualizar(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas SET 
                                            Nombre = @Nombre, 
                                            Balance = @Balance, 
                                            Descripcion = @Descripcion,
                                            TipoCuentaId = @TipoCuentaId
                                            WHERE Id = @Id", cuenta);
        }


        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Cuentas WHERE Id = @Id", new { id });
        }
    }
}
