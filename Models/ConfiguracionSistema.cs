namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ConfiguracionSistema
    {
        [Key]
        public int ConfiguracionId { get; set; }


        [Required]
        public string? Clave { get; set; }


        [Required]
        public string? Valor { get; set; }


        public string? Descripcion { get; set; }
    }
}
