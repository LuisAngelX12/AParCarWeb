namespace AParCarWeb.Models
{
    using AParCarWeb.Data;
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        public string? IdentityUserId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string? Nombre { get; set; }

        public string EstadoUsuario { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ApplicationUser? IdentityUser { get; set; }
    }
}