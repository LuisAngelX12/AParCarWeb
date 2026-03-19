namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ReporteGenerado
    {
        [Key]
        public int ReporteId { get; set; }


        [Required]
        public string? TipoReporte { get; set; }


        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
        public string? GeneradoPor { get; set; }
    }
}