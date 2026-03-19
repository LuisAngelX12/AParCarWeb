namespace AParCarWeb.Models.ViewModels.Vmod2
{
    using System.ComponentModel.DataAnnotations;

    public class VehiculoVM
    {
        public int VehiculoId { get; set; }

        [Required]
        public string? Placa { get; set; }

        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }

        // Clientes seleccionados
        public List<int> ClientesSeleccionados { get; set; } = new();
    }
}