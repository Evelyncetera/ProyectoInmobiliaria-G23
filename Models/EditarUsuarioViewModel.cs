using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public class EditarUsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = "";

        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol de Usuario")]
        public string Rol { get; set; } = "Empleado";
    }
}
