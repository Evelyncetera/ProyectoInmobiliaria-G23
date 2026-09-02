namespace Proyecto_Inmobiliaria.Models
{
    public class InmuebleConReservas
    {
        public int IdInmueble { get; set; }

        public string Direccion { get; set; } = "";

        public int CantidadReservas { get; set; }
    }
}