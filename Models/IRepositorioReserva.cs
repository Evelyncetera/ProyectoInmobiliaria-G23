using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public interface IRepositorioReserva
    {
        int Alta(Reserva r, int idUsuarioCreador);
        int Baja(int id, int idUsuarioAnulador); // Baja lógica (anula la reserva)
        int Modificacion(Reserva r);
        IList<Reserva> ObtenerTodos();
        Reserva? ObtenerPorId(int id);

        bool EstaDisponible(int idInmueble, DateTime desde, DateTime hasta, int? exceptoId = null);

        int TerminarAnticipadamente(int idReserva, DateTime fechaTerminacion, int idUsuarioTerminador);

        /* ----- Informes ----- */
        IList<Reserva> ObtenerVigentes(); 
        IList<Reserva> ObtenerPorTerminar(int dias); //reservas que finalizan dentro de X días
        IList<InmuebleConReservas> ObtenerMasReservados(); //en los últimos 365 días
        IList<Inmueble> ObtenerInmueblesSinReservas(int dias); 
        IList<Inmueble> ObtenerInmueblesDisponibles(DateTime desde, DateTime hasta); 
    }
}