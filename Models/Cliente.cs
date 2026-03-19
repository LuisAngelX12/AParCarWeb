namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required, StringLength(100)]
        public string? Nombre { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Required]
        [RegularExpression(
        @"^(\d{9}|\d{11,12})$",
        ErrorMessage = "Cédula inválida. Use 9 dígitos (nacional) o 11-12 (DIMEX)."
        )]
        public string? Identificacion { get; set; }

        public string? TipoCliente { get; set; }

        public string? Telefono { get; set; }
        public string? Email { get; set; }


        public ICollection<ClienteVehiculo>? ClienteVehiculos { get; set; }
    }
}