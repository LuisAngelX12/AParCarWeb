namespace AParCarWeb.ViewModels
{
    using AParCarWeb.Models;

    public class DashboardOperadorVM
    {
        public int EspaciosDisponibles { get; set; }

        public int EspaciosOcupados { get; set; }

        public int VehiculosDentro { get; set; }

        public int EntradasHoy { get; set; }

        public int SalidasHoy { get; set; }

        public List<Ticket> TicketsExcedidos { get; set; } = new();
    }
}