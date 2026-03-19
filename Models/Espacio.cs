namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Espacio
    {
        [Key]
        public int EspacioId { get; set; }

        [Required(ErrorMessage = "La zona es obligatoria")]
        public int ZonaId { get; set; }
        public Zona? Zona { get; set; }


        [Required(ErrorMessage = "El código es obligatorio")]
        public string? Codigo { get; set; }


        public bool Ocupado { get; set; }


        public ICollection<Ticket>? Tickets { get; set; }
    }
}