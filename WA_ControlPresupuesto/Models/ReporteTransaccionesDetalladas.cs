namespace WA_ControlPresupuesto.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }//Esta propiedad contendra una coleccion de objetos TransaccionesPorFecha que es una clase que definimos mas abajo
        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(t => t.BalanceDepositos);//Sumamos el balance de depositos de cada dia
        public decimal BalanceRetiros => TransaccionesAgrupadas.Sum(t => t.BalanceRetiros);//Sumamos el balance de retiros de cada dia
        //Puede confundir que abajo ya se esta sumando los depositos y retiros de cada dia, pero aqui estamos sumando el total de depositos y retiros de todo el periodo que abarca el reporte
        public decimal Total=> BalanceDepositos - BalanceRetiros;//El total es la resta de los depositos menos los retiros
        public class TransaccionesPorFecha
        {
            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }
            public decimal BalanceDepositos => Transacciones.Where(t => t.TipoOperacionId == TipoOperacion.Ingreso).Sum(t => t.Monto);//Lo que hace es sumar todos los ingresos de las transacciones de ese dia, si no hay ingresos devuelve 0. Estamos usando expresiones lambda, y LINQ
            public decimal BalanceRetiros => Transacciones.Where(t => t.TipoOperacionId == TipoOperacion.Gasto).Sum(t => t.Monto);//Aqui en lugar de ingresos, son gastos
        }
        
    }
}
