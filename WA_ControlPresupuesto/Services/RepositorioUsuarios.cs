using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario?> ObtenerUsuarioPorEmail(string emailNormalizado);
    }
    public class RepositorioUsuarios: IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int>CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
                                                              VALUES (@Email, @EmailNormalizado, @PasswordHash);
                                                              SELECT SCOPE_IDENTITY();", usuario);
            return id;//Devuelve el id del usuario que se acaba de crear
        }

        public async Task<Usuario?> ObtenerUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado", new { emailNormalizado });//Usamos QuerySingleOrDefaultAsync porque puede que no exista el usuario, en ese caso devuelve null. Si no colocamos esto y no existe el usuario, lanza una excepción, si usaramos pro ejemplo QuerySingleAsync y no existe el usuario, lanza una excepción. En este caso si no existe el usuario, devuelve null.
            //Porque colocamos Usuario? con el signo de interrogación, porque puede devolver null si no existe el usuario. 
        }
    }
}
