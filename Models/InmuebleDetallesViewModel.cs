namespace Proyecto_Inmobiliaria.Models
{
    public class InmuebleDetallesViewModel
    {
        public Inmueble Inmueble { get; set; } = new();
        public IList<ImagenInmueble> Imagenes { get; set; } = new List<ImagenInmueble>();
    }
}
