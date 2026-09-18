namespace Proyecto_Inmobiliaria.Models
{
    public class InmueblesListadoViewModel
    {
        public string? Buscar { get; set; }
        public int? Propietario { get; set; }
        public bool? Disponible { get; set; }
        public string Estado { get; set; } = "activos";
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
        public int Total { get; set; }
        public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)TamanoPagina));
        public IList<Inmueble> Items { get; set; } = new List<Inmueble>();
    }
}
