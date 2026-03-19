namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    public class Vehiculo
    {
        [Key]
        public int VehiculoId { get; set; }


        [Required(ErrorMessage = "La placa es obligatoria")]
        public string? Placa { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public string? Marca { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string? Modelo { get; set; }

        [Required(ErrorMessage = "El color es obligatorio")]
        public string? Color { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<ClienteVehiculo>? ClienteVehiculos { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }
    }
}