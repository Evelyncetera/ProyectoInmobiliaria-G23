namespace Proyecto_Inmobiliaria.Models;

    
public interface IRepositorioPago
{
    int Alta(Pago p, int idUsuarioCreador);
    int Baja(int id, int idUsuarioAnulador);
    int Modificacion(Pago p);
    IList<Pago> ObtenerTodos();
    Pago? ObtenerPorId(int id);

    IList<Pago> ObtenerPorReserva(int idReserva);

    int RegistrarPenalizacionYTerminarReserva(Pago pago, DateTime fechaTerminacion, int idUsuario);
}
