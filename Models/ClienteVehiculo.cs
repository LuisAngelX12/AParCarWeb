namespace AParCarWeb.Models
{

    public class ClienteVehiculo
    {
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}