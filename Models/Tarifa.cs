namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Tarifa
    {
        [Key]
        public int TarifaId { get; set; }


        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio por hora es obligatorio")]
        [Display(Name = "Precio por hora")]
        public decimal PrecioHora { get; set; }


        public ICollection<Pago>? Pagos { get; set; }
    }
}