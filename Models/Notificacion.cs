namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Notificacion
    {
        [Key]
        public int NotificacionId { get; set; }


        public string? Mensaje { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public bool Leida { get; set; }
    }
}