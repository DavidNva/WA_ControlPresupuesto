namespace WA_ControlPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }
        public decimal Balance => Cuentas.Sum(x => x.Balance);//Suma de los balances de todas las cuentas que hay en la lista que pertencen a ese tipo de cuenta.
    }
}
