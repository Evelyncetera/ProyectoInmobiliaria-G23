namespace Proyecto_Inmobiliaria.Models
{
    public class OpcionInmueble
    {
        public int Id { get; set; }
        public string Texto { get; set; }

        public OpcionInmueble(int id, string texto)
        {
            Id = id;
            Texto = texto;
        }
    }
}
