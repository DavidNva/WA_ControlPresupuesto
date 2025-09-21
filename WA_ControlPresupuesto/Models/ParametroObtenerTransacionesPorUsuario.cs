namespace WA_ControlPresupuesto.Models
{
    public class ParametroObtenerTransacionesPorUsuario
    {
        public int UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
