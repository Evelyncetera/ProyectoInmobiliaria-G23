using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public class Reserva
    {
        [Key]
        [Display(Name = "Código Int.")]
        public int IdReserva { get; set; } //pk

        [Required(ErrorMessage = "El inquilino es obligatorio.")]
        [Display(Name = "Inquilino")]
        public int IdInquilino { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio.")]
        [Display(Name = "Inmueble")]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha desde")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha hasta")]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "El monto por día es obligatorio.")]
        [Range(0.01, 999999999.99, ErrorMessage = "El monto por día debe ser mayor a 0.")]
        [Display(Name = "Monto por día")]
        public decimal MontoPorDia { get; set; }

        [Display(Name = "Anulada")]
        public bool Anulada { get; set; } = false;

        /* ----- Campos enriquecidos (se completan con JOINs en el repositorio) ----- */
        [Display(Name = "Nombre del inquilino")]
        public string NombreInquilino { get; set; } = "";

        [Display(Name = "Apellido del inquilino")]
        public string ApellidoInquilino { get; set; } = "";

        [Display(Name = "DNI del inquilino")]
        public string DniInquilino { get; set; } = "";

        [Display(Name = "Dirección del inmueble")]
        public string DireccionInmueble { get; set; } = "";

        public override string ToString()
        {
            var res = $"{NombreInquilino} {ApellidoInquilino} - {DireccionInmueble}";
            if (FechaDesde != DateTime.MinValue && FechaHasta != DateTime.MinValue)
            {
                res += $" ({FechaDesde:dd/MM/yyyy} a {FechaHasta:dd/MM/yyyy})";
            }
            return res;
        }
    }
}