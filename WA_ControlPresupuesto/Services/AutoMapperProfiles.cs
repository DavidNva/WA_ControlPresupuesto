using AutoMapper;
using WA_ControlPresupuesto.Models;

namespace WA_ControlPresupuesto.Services
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();//Esto quiere decir que mapee de Cuenta a CuentaCreacionViewModel
            //Ejemplo: Si Cuenta tiene una propiedad Nombre y CuentaCreacionViewModel tambien tiene una propiedad Nombre, entonces automaticamente mapea el valor de Nombre de Cuenta a Nombre de CuentaCreacionViewModel. 
            //Otro ejemplo para entender si o si: Si Cuenta tiene una propiedad TipoCuentaId y CuentaCreacionViewModel tambien tiene una propiedad TipoCuentaId, entonces automaticamente mapea el valor de TipoCuentaId de Cuenta a TipoCuentaId de CuentaCreacionViewModel
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();

        }
    }
}
