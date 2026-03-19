namespace AParCarWeb.Controllers
{
    using AParCarWeb.Data;
    using AParCarWeb.Services;
    using AParCarWeb.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = "Operador")]
    public class DashboardOperadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfiguracionService _config;

        public DashboardOperadorController(ApplicationDbContext context, ConfiguracionService config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Index()
        {

            var alertar = _config.ObtenerBool("AlertarTiempoExcedido", true);

            if (alertar)
            {
                ViewBag.VehiculosExcedidos = _context.Tickets
                    .Where(t => t.FechaSalida == null &&
                    DateTime.UtcNow > t.FechaEntrada.AddMinutes(
                        _config.ObtenerInt("TiempoMaximoEstadia")))
                    .ToList();
            }

            var totalEspacios = _context.Espacios.Count();
            var espaciosDisponibles = _context.Espacios.Count(e => !e.Ocupado);

            var hoyUtc = DateTime.UtcNow.Date;
            var mananaUtc = hoyUtc.AddDays(1);

            var ahora = DateTime.UtcNow;

            var tiempoMaximo = _config.ObtenerInt("TiempoMaximoEstadia", 720);

            var excedidos = _context.Tickets
                .Where(t => t.FechaSalida == null &&
                DateTime.UtcNow > t.FechaEntrada.AddMinutes(tiempoMaximo))
                .Select(t => new
                {
                    t.TicketId,
                    t.FechaEntrada,
                    Vehiculo = t.Vehiculo!.Placa,
                    Espacio = t.Espacio!.Codigo
                })
                .ToList();

            var model = new DashboardOperadorVM
            {
                EspaciosDisponibles = espaciosDisponibles,
                EspaciosOcupados = totalEspacios - espaciosDisponibles,

                VehiculosDentro = _context.Tickets
                    .Count(t => t.FechaSalida == null),

                EntradasHoy = _context.Tickets.Count(t =>
                    t.FechaEntrada >= hoyUtc &&
                    t.FechaEntrada < mananaUtc),

                SalidasHoy = _context.Tickets.Count(t =>
                    t.FechaSalida.HasValue &&
                    t.FechaSalida.Value >= hoyUtc &&
                    t.FechaSalida.Value < mananaUtc)
            };

            ViewBag.TicketsExcedidos = excedidos;

            return View(model);
        }
    }
}