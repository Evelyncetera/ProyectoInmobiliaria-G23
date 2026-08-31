namespace Proyecto_Inmobiliaria.Models;

    
public interface IRepositorioInmueble
{
    int Alta(Inmueble i); //Prioridad
    int Baja(int id); //Prioridad
    int Modificacion(Inmueble i); //Prioridad
    IList<Inmueble> ObtenerTodos();
    Inmueble? ObtenerPorId(int id);

    /* ----- Informes que se van a necesitar más adelante ------
● Listar todos los inmuebles y su dueño, que estén en el sistema. Permitir filtrar por disponibilidad (no de fechas, sino de la propiedad “Estado” o “Disponible”).
● Listar todos los inmuebles que le correspondan a un propietario específico.
● Listar los inmuebles más reservados en los últimos 365 días.
● Listar los inmuebles sin reservas en los últimos X días (30, 60, etc.).
    
    */
}