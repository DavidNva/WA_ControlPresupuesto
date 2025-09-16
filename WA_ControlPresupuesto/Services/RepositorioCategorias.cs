using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
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

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"
                                        SELECT * FROM Categorias
                                        WHERE UsuarioId = @UsuarioId
                                        ORDER BY Nombre", new { usuarioId });//El new { usuarioId } es un objeto anonimo que se usa para pasar parametros a la consulta. Es decir, es como si hicieramos new { UsuarioId = usuarioId }, pero en C# si el nombre de la propiedad es igual al nombre de la variable, podemos omitirlo.
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"
                                        SELECT * FROM Categorias
                                        WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @tipoOperacionId
                                        ORDER BY Nombre", new { usuarioId, tipoOperacionId });//El new { usuarioId } es un objeto anonimo que se usa para pasar parametros a la consulta. Es decir, es como si hicieramos new { UsuarioId = usuarioId }, pero en C# si el nombre de la propiedad es igual al nombre de la variable, podemos omitirlo.
        }


        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            //Este método es para obtener una categoría por su id y el id del usuario, para asegurarnos de que el usuario solo pueda ver sus propias categorías. El cual lo usaremos para editar una categoría, donde primero obtendremos la categoría por su id y el id del usuario, y luego mostraremos el formulario de edición con los datos de la categoría. en caso de que la categoría no exista o no pertenezca al usuario, retornaremos vista de no encontrado. Este metodo es de validacion de seguridad.
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"
                                        SELECT * FROM Categorias
                                        WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId });//El new { id, usuarioId } es un objeto anonimo que se usa para pasar parametros a la consulta. Es decir, es como si hicieramos new { Id = id, UsuarioId = usuarioId }, pero en C# si el nombre de la propiedad es igual al nombre de la variable, podemos omitirlo.
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                                        UPDATE Categorias
                                        SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                                        WHERE Id = @Id AND UsuarioId = @UsuarioId", categoria);//Usamos ExecuteAsync porque no esperamos ningun resultado, solo queremos ejecutar la consulta.  
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM Categorias WHERE Id = @Id", new { id });
        }
    }
}
