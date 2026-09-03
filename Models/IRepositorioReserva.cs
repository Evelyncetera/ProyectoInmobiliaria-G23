using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public interface IRepositorioReserva
    {
        int Alta(Reserva r);
        int Baja(int id); //Baja lógica (anula la reserva)
        int Modificacion(Reserva r);
        IList<Reserva> ObtenerTodos();
        Reserva? ObtenerPorId(int id);

   
        bool EstaDisponible(int idInmueble, DateTime desde, DateTime hasta, int? exceptoId = null);

        /* ----- Informes ----- */
        IList<Reserva> ObtenerVigentes(); 
        IList<Reserva> ObtenerPorTerminar(int dias); //reservas que finalizan dentro de X días
        IList<InmuebleConReservas> ObtenerMasReservados(); //en los últimos 365 días
        IList<Inmueble> ObtenerInmueblesSinReservas(int dias); 
        IList<Inmueble> ObtenerInmueblesDisponibles(DateTime desde, DateTime hasta); 
    }
}