namespace Proyecto_Inmobiliaria.Models;

    
public interface IRepositorioTipoInmueble
{
    int Alta(TipoInmueble t); //Prioridad
    int Baja(int id); //Prioridad
    int Modificacion(TipoInmueble t); //Prioridad
    IList<TipoInmueble> ObtenerTodos();
    TipoInmueble? ObtenerPorId(int id);
}