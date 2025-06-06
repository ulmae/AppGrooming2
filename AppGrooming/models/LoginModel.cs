using System.ComponentModel.DataAnnotations;

namespace AppGrooming.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        [Display(Name = "Usuario")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseņa es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseņa")]
        public string Password { get; set; }
    }
}