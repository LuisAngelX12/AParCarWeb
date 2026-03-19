using AParCarWeb.Data;
using AParCarWeb.Models;
using AParCarWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Admin,Operador")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificacionService _notificacion;
        private readonly ConfiguracionService _config;

        public TicketsController(ApplicationDbContext context, NotificacionService notificacion, ConfiguracionService config)
        {
            _context = context;
            _notificacion = notificacion;
            _config = config;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(string estado, DateTime? desde, DateTime? hasta)
        {
            var tickets = _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo!)
                    .ThenInclude(v => v.ClienteVehiculos!)
                        .ThenInclude(cv => cv.Cliente)
                .AsQueryable();

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado))
                tickets = tickets.Where(t => t.Estado == estado);

            // Filtro por rango de fechas
            if (desde.HasValue)
                tickets = tickets.Where(t => t.FechaEntrada >= desde.Value);
            if (hasta.HasValue)
                tickets = tickets.Where(t => t.FechaEntrada <= hasta.Value);

            // Pasar valores al ViewBag
            ViewBag.EstadoFiltro = estado;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            ViewData["EstadoList"] = new SelectList(new[] { "Activo", "Pagado" }, ViewBag.EstadoFiltro as string);

            return View(await tickets.OrderByDescending(t => t.FechaEntrada).ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            // Solo espacios libres
            var espaciosLibres = _context.Espacios.Where(e => !e.Ocupado).ToList();
            ViewData["EspacioId"] = new SelectList(espaciosLibres, "EspacioId", "Codigo");

            // Vehículos libres (sin ticket activo)
            var vehiculosUsados = _context.Tickets
                .Where(t => t.Estado == "Activo")
                .Select(t => t.VehiculoId)
                .ToList();
            var vehiculosLibres = _context.Vehiculos
                .Where(v => !vehiculosUsados.Contains(v.VehiculoId))
                .ToList();
            ViewData["VehiculoId"] = new SelectList(vehiculosLibres, "VehiculoId", "Placa");

            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,VehiculoId,EspacioId,FechaEntrada,FechaSalida,Estado")] Ticket ticket)
        {
            var capacidadMax = _config.ObtenerInt("CapacidadMaxima", 50);
            var ocupados = _context.Espacios.Count(e => e.Ocupado);

            if (ocupados >= capacidadMax)
            {
                TempData["Error"] = "El parqueo está lleno.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                // Fechas a UTC
                ticket.FechaEntrada = DateTime.SpecifyKind(ticket.FechaEntrada, DateTimeKind.Utc);
                if (ticket.FechaSalida.HasValue)
                    ticket.FechaSalida = DateTime.SpecifyKind(ticket.FechaSalida.Value, DateTimeKind.Utc);

                // Estado inicial
                ticket.Estado = "Activo";

                // Marcar espacio como ocupado
                var espacio = await _context.Espacios.FindAsync(ticket.EspacioId);
                if (espacio != null) espacio.Ocupado = true;

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                _notificacion.Crear("Nuevo vehículo ingresó al parqueo.");
                return RedirectToAction(nameof(Index));
            }

            // Recargar combos si falla ModelState
            var espaciosLibres = _context.Espacios.Where(e => !e.Ocupado).ToList();
            ViewData["EspacioId"] = new SelectList(espaciosLibres, "EspacioId", "Codigo", ticket.EspacioId);

            var vehiculosUsados = _context.Tickets
                .Where(t => t.Estado == "Activo")
                .Select(t => t.VehiculoId)
                .ToList();
            var vehiculosLibres = _context.Vehiculos
                .Where(v => !vehiculosUsados.Contains(v.VehiculoId))
                .ToList();
            ViewData["VehiculoId"] = new SelectList(vehiculosLibres, "VehiculoId", "Placa", ticket.VehiculoId);

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ViewData["EspacioId"] = new SelectList(_context.Espacios, "EspacioId", "Codigo", ticket.EspacioId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "VehiculoId", "Placa", ticket.VehiculoId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,VehiculoId,EspacioId,FechaEntrada,FechaSalida,Estado")] Ticket ticket)
        {
            if (id != ticket.TicketId) return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    ticket.FechaEntrada = DateTime.SpecifyKind(ticket.FechaEntrada, DateTimeKind.Utc);
                    if (ticket.FechaSalida.HasValue)
                        ticket.FechaSalida = DateTime.SpecifyKind(ticket.FechaSalida.Value, DateTimeKind.Utc);

                    // Cambiar estado según FechaSalida
                    ticket.Estado = ticket.FechaSalida.HasValue ? "Pagado" : "Activo";

                    // Liberar espacio si hay salida
                    var espacio = await _context.Espacios.FindAsync(ticket.EspacioId);
                    if (ticket.FechaSalida.HasValue && espacio != null)
                        espacio.Ocupado = false;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["EspacioId"] = new SelectList(_context.Espacios, "EspacioId", "Codigo", ticket.EspacioId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "VehiculoId", "Placa", ticket.VehiculoId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                // Liberar espacio si se borra
                var espacio = await _context.Espacios.FindAsync(ticket.EspacioId);
                if (espacio != null) espacio.Ocupado = false;

                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }
    }
}