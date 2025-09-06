using Dapper;
using Microsoft.Data.SqlClient;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //Aplicando programación asincrona
        //TASK ASI SOLO es como un void, como un void asincrona
        public async Task Crear(TipoCuenta tipoCuenta)
        {//para hacer una espera asincrona del query
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("sp_TiposCuentas_Insertar",
                                            new {usuarioId = tipoCuenta.UsuarioId,
                                            nombre = tipoCuenta.Nombre},
                                            commandType: System.Data.CommandType.StoredProcedure );

            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            //traeme el primero o valor por defecto del tipo de dato colocado, (para int es 0)
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas CUENTAS    
                                            WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId",
                                            new { nombre, usuarioId });//Con dapper de esta forma indicamos que va en @Nombre y @UsuarioId

            //Eso traerá 1 si existe un registro con esos datos o un 0 si no existe 
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden 
                                                            FROM TiposCuentas
                                                            Where UsuarioId = @UsuarioId ORDER BY Orden", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas 
                                            SET Nombre = @Nombre
                                            WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"
                                                                SELECT Id, Nombre, Orden
                                                                FROM TiposCuentas
                                                                WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId });

        }


        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE Id = @Id", new { id });
            //Lo ponemos asi, porque al usar dapper, si ponemos tipoCuenta.Id nos dara error
            //Entonces, pones new { id } y dapper entiende que es un objeto anonimo y que id va en @id
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
        }//Aunque en el metodo no estemos viendo ningun foreach, dapper internamente hace un foreach por cada elemento del IEnumerable y ejecuta el query por cada uno de ellos, pasando los valores de Id y Orden de cada elemento 
        //Esto lo hace o sabe que lo debe hacer asi porque le pasamos un IEnumerable, si le pasas un objeto normal, entonces solo lo ejecuta una vez
    }
}
