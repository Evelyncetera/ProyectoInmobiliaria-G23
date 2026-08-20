using System.ComponentModel.DataAnnotations;

public class Inquilino
{
    [Key]
    [Display(Name = "Código Int.")]
    public int IdInquilino {get; set;} //pk

    [Required]
    public String Dni {get; set; } = "";
    
    [Required]
    public String Nombre {get; set;} = "";
    
    [Required]
    public String Apellido {get; set;} = "";
    
    [Display(Name = "Telefono")]
    public String Telefono {get; set;} = "";

    [Required, EmailAddress]
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