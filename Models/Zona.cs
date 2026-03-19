namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Zona
    {
        [Key]
        public int ZonaId { get; set; }


        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string? Descripcion { get; set; }


        public ICollection<Espacio>? Espacios { get; set; }
    }
}