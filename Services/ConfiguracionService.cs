namespace AParCarWeb.Services
{
    using AParCarWeb.Data;

    public class ConfiguracionService
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener valor string
        public string Obtener(string clave, string defecto = "")
        {
            var config = _context.ConfiguracionesSistema
                .FirstOrDefault(c => c.Clave == clave);

            return config?.Valor ?? defecto;
        }

        // Obtener entero
        public int ObtenerInt(string clave, int defecto = 0)
        {
            var valor = Obtener(clave);

            return int.TryParse(valor, out int resultado)
                ? resultado
                : defecto;
        }

        // Obtener decimal
        public decimal ObtenerDecimal(string clave, decimal defecto = 0)
        {
            var valor = Obtener(clave);

            return decimal.TryParse(valor, out decimal resultado)
                ? resultado
                : defecto;
        }

        // Obtener booleano
        public bool ObtenerBool(string clave, bool defecto = false)
        {
            var valor = Obtener(clave);

            return bool.TryParse(valor, out bool resultado)
                ? resultado
                : defecto;
        }
    }
}