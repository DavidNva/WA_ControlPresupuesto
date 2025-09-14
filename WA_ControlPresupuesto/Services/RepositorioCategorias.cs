using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                                        INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                                        VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
                                        SELECT SCOPE_IDENTITY();", categoria);//Usamos QuerySingleAsync<int> porque esperamos un unico resultado, que es el id que se acaba de insertar. Si no devolviera nada, usariamos ExecuteAsync.
            categoria.Id = id;
        }

    }
}
