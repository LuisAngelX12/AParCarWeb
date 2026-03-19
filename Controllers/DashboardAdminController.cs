namespace AParCarWeb.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using AParCarWeb.Data;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class DashboardAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD ADMIN
        public IActionResult Admin()
        {
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.TotalVehiculos = _context.Vehiculos.Count();

            var totalEspacios = _context.Espacios.Count();
            var espaciosDisponibles = _context.Espacios.Count(e => !e.Ocupado);
            var espaciosOcupados = totalEspacios - espaciosDisponibles;

            ViewBag.EspaciosDisponibles = espaciosDisponibles;

            // Ocupación %
            ViewBag.PorcentajeOcupacion = totalEspacios == 0
                ? 0
                : Math.Round((decimal)espaciosOcupados / totalEspacios * 100, 2);

            var today = DateTime.UtcNow.Date;

            ViewBag.IngresosHoy = _context.Pagos
                .Where(p => p.FechaPago >= today && p.FechaPago < today.AddDays(1))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            ViewBag.VehiculosDentro = _context.Tickets
                .Count(t => t.FechaSalida == null);

            ViewBag.NotificacionesNoLeidas = _context.Notificaciones
                .Where(n => !n.Leida)
                .OrderByDescending(n => n.Fecha)
                .Take(5)
                .ToList();

            ViewBag.ReportesRecientes = _context.ReportesGenerados
                .OrderByDescending(r => r.FechaGeneracion)
                .Take(5)
                .ToList();

            ViewBag.ConfiguracionesSistema = _context.ConfiguracionesSistema
                .OrderBy(c => c.Clave)
                .ToList();

            return View();
        }
    }
}
