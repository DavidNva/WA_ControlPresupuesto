namespace WA_ControlPresupuesto.Models
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 10;

        private readonly int cantidadMaximaRecordsPorPagina = 50;

        public int RecordsPorPagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > cantidadMaximaRecordsPorPagina) ? cantidadMaximaRecordsPorPagina : value;// Asigna el valor, pero no puede ser mayor a la cantidad máxima
            }
        }

        public int RecordsASaltar => recordsPorPagina * (Pagina - 1);
    }
}
