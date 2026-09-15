using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = "";

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = "";

        public string? AvatarUrl { get; set; }

        public IFormFile? Avatar { get; set; }
    }

    public class CambiarClaveViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string ClaveActual { get; set; } = "";

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string ClaveNueva { get; set; } = "";

        [Compare(nameof(ClaveNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarClave { get; set; } = "";

        public PerfilViewModel? Perfil { get; set; }
    }
}