using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public class Inmueble
    {
        [Key]
        [Display(Name = "Código Int.")]
        public int IdInmueble {get; set;} //pk
        
        [Required(ErrorMessage = "El propietario es obligatorio.")]
        [Display(Name = "Propietario")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "El tipo de inmueble es obligatorio.")]
        [Display(Name = "Tipo de Inmueble")]
        public int IdTipoInmueble { get; set;}

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres.")]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "El cupo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor a 0.")]
        public int Cupo { get; set; }

        [Required(ErrorMessage = "La latitud es obligatoria.")]
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public decimal Latitud { get; set; }

        [Required(ErrorMessage = "La longitud es obligatoria.")]
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public decimal Longitud { get; set; }

        [Required(ErrorMessage = "El precio por día es obligatorio.")]
        [Range(0.01, 999999999.99, ErrorMessage = "El precio por día debe ser mayor a 0.")]
        [Display(Name = "Precio por día")]
        public decimal PrecioPorDia { get; set; }

        [Required(ErrorMessage = "El porcentaje de reserva es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Display(Name = "Porcentaje de reserva")]
        public decimal PorcentajeReserva { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        public override string ToString()
        {
            return $"{Direccion} - {PrecioPorDia:C}/día";
        }
    }
}
