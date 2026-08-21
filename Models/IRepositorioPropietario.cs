namespace Proyecto_Inmobiliaria.Models;

    
public interface IRepositorioPropietario
{
    int Alta(Propietario p); //Prioridad
    int Baja(int id); //Prioridad
    int Modificacion(Propietario p); //Prioridad
    IList<Propietario> ObtenerTodos();
    Propietario? ObtenerPorId(int id);

/*     Propietario? ObtenerPorMail(string email); */

    /* IList<Propietario> BuscarPorNombre(string nombre); */
}
