namespace Proyecto_Inmobiliaria.Models;


public interface IRepositorioInquilino
{
    int Alta(Inquilino p); //Prioridad
    int Baja(int id); //Prioridad
    int Modificacion(Inquilino p); //Prioridad
    IList<Inquilino> ObtenerTodos();
    Inquilino? ObtenerPorId(int id);

 /*    Inquilino? ObtenerPorMail(string email); */

/*     IList<Inquilino> BuscarPorNombre(string nombre); */
}