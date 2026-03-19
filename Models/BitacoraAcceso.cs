namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class BitacoraAcceso
    {
        [Key]
        public int BitacoraId { get; set; }


        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }


        public DateTime FechaHora { get; set; } = DateTime.UtcNow;


        [Required]
        public string? Accion { get; set; }


        public string? IP { get; set; }
    }
}