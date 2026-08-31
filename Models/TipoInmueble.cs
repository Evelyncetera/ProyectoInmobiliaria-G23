using System.ComponentModel.DataAnnotations;

public class TipoInmueble
{
    [Key]
    [Display(Name = "Código Int.")]
    public int IdTipoInmueble {get; set;} //pk
    
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public String Nombre {get; set;} = "";
    
    public override string ToString()
    {
			return Nombre;
		}
}