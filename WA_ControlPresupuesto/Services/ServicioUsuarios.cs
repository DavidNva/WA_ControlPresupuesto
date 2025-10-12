using System.Security.Claims;

namespace WA_ControlPresupuesto.Services
{
    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public int ObtenerUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                //solamente si el usuario esta autenticado
                var idClaim = httpContext.User
                    .Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();//Obtenemos el id del usuario real

                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");
                //esto es si el usuario no esta autenticado
            }
            return 1;
        }
    }
}
