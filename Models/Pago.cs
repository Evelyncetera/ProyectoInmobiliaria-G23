using System.ComponentModel.DataAnnotations;

namespace Proyecto_Inmobiliaria.Models
{
    public class Pago
    {
        [Key]
        [Display(Name = "Código Int.")]
        public int IdPago { get; set; } //pk
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "La seña es obligatoria")]
        [Display(Name = "Seña")]
        public string Concepto { get; set; }

        [Required(ErrorMessage = "La fecha de pago es importante")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de pago")]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        public bool Anulada { get; set; } = false;

        public int? IdUsuarioCreador { get; set; }
        public string? NombreUsuarioCreador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? IdUsuarioAnulador { get; set; }
        public string? NombreUsuarioAnulador { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        public override string ToString()
        {
            return "";
        }
    }
}
