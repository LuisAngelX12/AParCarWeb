namespace AParCarWeb.Models.ViewModels.Vmod1
{
    using System.ComponentModel.DataAnnotations;

    public class UsuarioEditVM
    {
        public int UsuarioId { get; set; }

        [Required]
        public string? Nombre { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; } // <- Añadido

        [Required]
        public string? Rol { get; set; }

        public string? EstadoUsuario { get; set; }

        public string? IdentityUserId { get; set; }
    }
}