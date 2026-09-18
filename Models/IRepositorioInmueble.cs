namespace Proyecto_Inmobiliaria.Models
{
    public interface IRepositorioInmueble
    {
        int Alta(Inmueble i);
        int Baja(int id);
        int Modificacion(Inmueble i);
        IList<Inmueble> ObtenerTodos();
        Inmueble? ObtenerPorId(int id);
        InmueblesListadoViewModel ObtenerPagina(InmueblesListadoViewModel filtro);
        int Guardar(Inmueble inmueble, IReadOnlyList<string> imagenes, bool nuevo);
        bool CambiarDisponibilidad(int id, bool disponible);
        IList<OpcionInmueble> BuscarPropietarios(string? buscar, int? seleccionado = null);
        IList<OpcionInmueble> BuscarTipos(string? buscar, int? seleccionado = null);
        IList<ImagenInmueble> ObtenerImagenes(int idInmueble);
        string? ObtenerImagenBase64(int id);
        bool CambiarPortada(int idInmueble, int idImagen);
        bool BajaImagen(int idInmueble, int idImagen);
    }
}
