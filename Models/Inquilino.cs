using System.ComponentModel.DataAnnotations;

public class Inquilino
{
    [Key]
    [Display(Name = "Código Int.")]
    public int IdInquilino {get; set;} //pk

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener entre 7 y 8 números.")]
    public String Dni {get; set; } = "";
    
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?:[ '-][A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)*$", ErrorMessage = "El nombre solo puede contener letras y separadores válidos.")]
    public String Nombre {get; set;} = "";
    
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?:[ '-][A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)*$", ErrorMessage = "El apellido solo puede contener letras y separadores válidos.")]
    public String Apellido {get; set;} = "";
    
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Display(Name = "Telefono")]
    [RegularExpression(@"^[0-9+()\s-]{7,20}$", ErrorMessage = "El teléfono contiene caracteres no válidos.")]
    public String Telefono {get; set;} = "";

    [Required(ErrorMessage = "El email es obligatorio."), EmailAddress(ErrorMessage = "Ingrese un email válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public String Email {get; set;} = "";

    public override string ToString()
    {
			//=> $"{Nombre} {Apellido} ({Dni})";
			var res = $"{Nombre} {Apellido}";
			if(!String.IsNullOrEmpty(Dni)) {
				res += $" ({Dni})";
			}
			return res;
		}
}