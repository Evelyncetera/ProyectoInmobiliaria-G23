namespace Proyecto_Inmobiliaria.Models
{
    public class ImagenInmueble
    {
        public int Id { get; set; }
        public bool EsPortada { get; set; }

        public ImagenInmueble(int id, bool esPortada)
        {
            Id = id;
            EsPortada = esPortada;
        }
    }
}
