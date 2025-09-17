namespace WA_ControlPresupuesto.Models
{
    public class TransaccionActualizacionViewModel:TransaccionCreacionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
    }
}
