namespace Proyecto_Inmobiliaria.Models
{
    public class ReservaDetalleViewModel
    {
        public Reserva Reserva { get; set; }
        public IList<Pago> Pagos { get; set; }
    }
}