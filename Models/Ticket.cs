namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria")]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "El espacio es obligatorio")]
        public int EspacioId { get; set; }
        public Espacio? Espacio { get; set; }

        [Required(ErrorMessage = "La fecha de entrada es obligatoria")]
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string? Estado { get; set; }

        public Pago? Pago { get; set; }
    }
}