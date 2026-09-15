namespace Proyecto_Inmobiliaria.Models
{
    public interface IRepositorioUsuario
    {
        int Alta(Usuario u);
        int Baja(int id);
        int Modificacion(Usuario u);
        IList<Usuario> ObtenerTodos();
        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorEmail(string email);
        int CambiarClave(int idUsuario, string claveNueva);
        int ActualizarAvatar(int idUsuario, string avatarUrl);
        int ActualizarPerfil(int idUsuario, string nombre, string apellido, string email);
    }
}