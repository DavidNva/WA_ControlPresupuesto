namespace WA_ControlPresupuesto.Models
{
    public class PaginacionRespuesta
    {
        public int Pagina { get; set; } = 1;
        public int RecordsPorPagina { get; set; } = 10;
        public int CantidadTotalRecords { get; set; } //Es decir en este caso con categorias es el total de categorias que tiene el usuario

        //100 /25 son 4 paginas, etc
        public int CantidadTotalPaginas => (int)Math.Ceiling((double)CantidadTotalRecords / RecordsPorPagina);
        // Math.Ceiling redondea hacia arriba, es decir si tenemos 11 categorias y 10 por pagina, nos da 1.1, pero como no podemos tener 0.1 de pagina, redondea a 2 paginas.

        public string BaseUrl { get; set; }//Porque podemos reutilizarlo en diferentes endpoints, como categorias, cuentas, transacciones, etc.

        
    }

    public class PaginacionRespuesta<T> : PaginacionRespuesta
    {
        public IEnumerable<T> Elementos { get; set; }//Records
    }
}
