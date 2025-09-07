using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
    }

    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
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
