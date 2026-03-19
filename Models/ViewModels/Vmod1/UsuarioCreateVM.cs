namespace AParCarWeb.Models.ViewModels.Vmod1
{
    using System.ComponentModel.DataAnnotations;

    public class UsuarioCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        public string? Rol { get; set; }
    }
}