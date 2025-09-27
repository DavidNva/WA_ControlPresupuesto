using Microsoft.AspNetCore.Identity;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public class UsuarioStore : IUserStore<Usuario>, IUserEmailStore<Usuario>, IUserPasswordStore<Usuario>
    {
        private readonly IRepositorioUsuarios _repositorioUsuario;

        //Esta interfaz es para definir los métodos que se van a utilizar para manejar los usuarios
        //El primero: IUserStore<Usuario> es para manejar las operaciones básicas de un usuario (crear, eliminar, actualizar, buscar por id y nombre)
        //El segundo: IUserEmailStore<Usuario> es para manejar las operaciones relacionadas con el email del usuario (obtener, establecer, confirmar, buscar por email)
        //El tercero: IUserPasswordStore<Usuario> es para manejar las operaciones relacionadas con la contraseña del usuario (obtener, establecer, verificar si tiene contraseña)

        public UsuarioStore(IRepositorioUsuarios repositorioUsuario)
        {
            _repositorioUsuario = repositorioUsuario;
        }


        public async Task<IdentityResult> CreateAsync(Usuario user, CancellationToken cancellationToken)
        {
            user.Id = _repositorioUsuario.CrearUsuario(user).Result;
            return IdentityResult.Success;//Si se crea el usuario, devuelve éxito, si id no se crea, lanza una excepción, no devuelve null es decir, si user.id es 0 o null, lanza una excepción.
        }

        public Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //No hay recursos que liberar en este caso, pero es necesario implementar el método Dispose porque la interfaz lo requiere.
        }

        public async Task<Usuario?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _repositorioUsuario.ObtenerUsuarioPorEmail(normalizedEmail);
        }

        public Task<Usuario?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _repositorioUsuario.ObtenerUsuarioPorEmail(normalizedUserName);
        }

        public Task<string?> GetEmailAsync(Usuario user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedEmailAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedUserNameAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetPasswordHashAsync(Usuario user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(Usuario user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());//Convertimos el id a string porque el método devuelve un string
        }

        public Task<string?> GetUserNameAsync(Usuario user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> HasPasswordAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(Usuario user, string? email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(Usuario user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.EmailNormalizado = normalizedEmail;
            return Task.CompletedTask;//No es necesario usar async/await porque no hay operaciones asíncronas aquí, simplemente asignamos el valor y devolvemos una tarea completada.
        }

        public Task SetNormalizedUserNameAsync(Usuario user, string? normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(Usuario user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;//No es necesario usar async/await porque no hay operaciones asíncronas aquí, simplemente asignamos el valor y devolvemos una tarea completada.
        }

        public Task SetUserNameAsync(Usuario user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
