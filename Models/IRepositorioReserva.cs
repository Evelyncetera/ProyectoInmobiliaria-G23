using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public interface IRepositorioReserva
    {
        int Alta(Reserva r); //Prioridad
        int Baja(int id); //Baja lógica (anula la reserva)
        int Modificacion(Reserva r); //Prioridad
        IList<Reserva> ObtenerTodos();
        Reserva? ObtenerPorId(int id);

        /* Verifica que el inmueble no esté ocupado en el rango por otra reserva no anulada */
        bool EstaDisponible(int idInmueble, DateTime desde, DateTime hasta, int? exceptoId = null);

        /* ----- Informes ----- */
        IList<Reserva> ObtenerVigentes(); //fecha actual dentro de [desde, hasta]
        IList<Reserva> ObtenerPorTerminar(int dias); //reservas que finalizan dentro de X días
        IList<InmuebleConReservas> ObtenerMasReservados(); //top inmuebles reservados en los últimos 365 días
        IList<Inmueble> ObtenerInmueblesSinReservas(int dias); //inmuebles sin reservas en los últimos X días
        IList<Inmueble> ObtenerInmueblesDisponibles(DateTime desde, DateTime hasta); //libres entre dos fechas
    }
}