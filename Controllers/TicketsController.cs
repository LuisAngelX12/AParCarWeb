using AParCarWeb.Data;
using AParCarWeb.Models;
using AParCarWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Admin,Operador")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificacionService _notificacion;
        private readonly ConfiguracionService _config;

        public TicketsController(
            ApplicationDbContext context,
            NotificacionService notificacion,
            ConfiguracionService config)
        {
            _context = context;
            _notificacion = notificacion;
            _config = config;
        }

        // ============================================================
        // GET: Tickets
        // ============================================================
        public async Task<IActionResult> Index(
            string estado,
            DateTime? desde,
            DateTime? hasta)
        {
            var tickets = _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo!)
                    .ThenInclude(v => v.ClienteVehiculos!)
                        .ThenInclude(cv => cv.Cliente)
                .AsQueryable();

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado))
            {
                tickets = tickets.Where(t => t.Estado == estado);
            }

            // Filtro por rango de fechas
            if (desde.HasValue)
            {
                tickets = tickets.Where(t => t.FechaEntrada >= desde.Value);
            }

            if (hasta.HasValue)
            {
                tickets = tickets.Where(t => t.FechaEntrada <= hasta.Value);
            }

            ViewBag.EstadoFiltro = estado;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            ViewData["EstadoList"] = new SelectList(
                new[] { "Activo", "Pagado" },
                ViewBag.EstadoFiltro as string
            );

            return View(
                await tickets
                    .OrderByDescending(t => t.FechaEntrada)
                    .ToListAsync()
            );
        }

        // ============================================================
        // GET: Tickets/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // ============================================================
        // GET: Tickets/Create
        // ============================================================
        public IActionResult Create()
        {
            CargarCombos();

            return View();
        }

        // ============================================================
        // POST: Tickets/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TicketId,VehiculoId,EspacioId")]
            Ticket ticket)
        {
            // ========================================================
            // VALIDAR CAPACIDAD
            // ========================================================

            var capacidadMax = _config.ObtenerInt(
                "CapacidadMaxima",
                50
            );

            var ocupados = await _context.Espacios
                .CountAsync(e => e.Ocupado);

            if (ocupados >= capacidadMax)
            {
                TempData["Error"] = "El parqueo está lleno.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // VALIDAR MODELO
            // ========================================================

            if (ModelState.IsValid)
            {
                CargarCombos(
                    ticket.VehiculoId,
                    ticket.EspacioId
                );

                return View(ticket);
            }

            // ========================================================
            // VALIDAR VEHÍCULO
            // ========================================================

            var vehiculoExiste = await _context.Vehiculos
                .AnyAsync(v => v.VehiculoId == ticket.VehiculoId);

            if (!vehiculoExiste)
            {
                ModelState.AddModelError(
                    "VehiculoId",
                    "El vehículo seleccionado no existe."
                );

                CargarCombos(
                    ticket.VehiculoId,
                    ticket.EspacioId
                );

                return View(ticket);
            }

            // ========================================================
            // VALIDAR ESPACIO
            // ========================================================

            var espacio = await _context.Espacios
                .FirstOrDefaultAsync(
                    e => e.EspacioId == ticket.EspacioId
                );

            if (espacio == null)
            {
                ModelState.AddModelError(
                    "EspacioId",
                    "El espacio seleccionado no existe."
                );

                CargarCombos(
                    ticket.VehiculoId,
                    ticket.EspacioId
                );

                return View(ticket);
            }

            if (espacio.Ocupado)
            {
                ModelState.AddModelError(
                    "EspacioId",
                    "El espacio seleccionado ya está ocupado."
                );

                CargarCombos(
                    ticket.VehiculoId,
                    ticket.EspacioId
                );

                return View(ticket);
            }

            // ========================================================
            // VALIDAR QUE EL VEHÍCULO NO TENGA TICKET ACTIVO
            // ========================================================

            var vehiculoTieneTicketActivo =
                await _context.Tickets.AnyAsync(
                    t =>
                        t.VehiculoId == ticket.VehiculoId &&
                        t.Estado == "Activo"
                );

            if (vehiculoTieneTicketActivo)
            {
                ModelState.AddModelError(
                    "VehiculoId",
                    "Este vehículo ya tiene un ticket activo."
                );

                CargarCombos(
                    ticket.VehiculoId,
                    ticket.EspacioId
                );

                return View(ticket);
            }

            // ========================================================
            // FECHA DE ENTRADA
            // ========================================================
            //
            // IMPORTANTE:
            //
            // DateTime.UtcNow obtiene el instante actual en UTC.
            //
            // PostgreSQL guarda ese instante.
            //
            // Cuando lo mostramos usamos ToCR12H()
            // para convertirlo a Costa Rica.
            //
            // ========================================================

            ticket.FechaEntrada = DateTime.UtcNow;

            // Un ticket nuevo siempre comienza activo.
            ticket.FechaSalida = null;
            ticket.Estado = "Activo";

            // ========================================================
            // MARCAR ESPACIO COMO OCUPADO
            // ========================================================

            espacio.Ocupado = true;

            // ========================================================
            // GUARDAR
            // ========================================================

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

            // ========================================================
            // NOTIFICACIÓN
            // ========================================================

            _notificacion.Crear(
                "Nuevo vehículo ingresó al parqueo."
            );

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Tickets/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["EspacioId"] = new SelectList(
                _context.Espacios,
                "EspacioId",
                "Codigo",
                ticket.EspacioId
            );

            ViewData["VehiculoId"] = new SelectList(
                _context.Vehiculos,
                "VehiculoId",
                "Placa",
                ticket.VehiculoId
            );

            return View(ticket);
        }

        // ============================================================
        // POST: Tickets/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("TicketId,VehiculoId,EspacioId,FechaEntrada,FechaSalida,Estado")]
            Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            // ========================================================
            // IMPORTANTE:
            // Antes tenías:
            //
            // if (!ModelState.IsValid)
            //
            // Eso estaba invertido.
            // ========================================================

            if (ModelState.IsValid)
            {
                try
                {
                    // ------------------------------------------------
                    // La FechaEntrada ya está almacenada en UTC.
                    // NO debemos volver a convertirla.
                    // ------------------------------------------------

                    // ------------------------------------------------
                    // Si se está registrando una salida manualmente,
                    // asegurarnos de que se interprete correctamente.
                    //
                    // Si el formulario devuelve una fecha sin zona,
                    // la interpretamos como Costa Rica y la convertimos
                    // a UTC.
                    // ------------------------------------------------

                    if (ticket.FechaSalida.HasValue)
                    {
                        var fechaSalidaCR = DateTime.SpecifyKind(
                            ticket.FechaSalida.Value,
                            DateTimeKind.Unspecified
                        );

                        ticket.FechaSalida =
                            TimeZoneInfo.ConvertTimeToUtc(
                                fechaSalidaCR,
                                TimeZoneInfo.FindSystemTimeZoneById(
                                    "Central America Standard Time"
                                )
                            );

                        ticket.Estado = "Pagado";
                    }
                    else
                    {
                        ticket.Estado = "Activo";
                    }

                    // ------------------------------------------------
                    // Buscar el espacio
                    // ------------------------------------------------

                    var espacio = await _context.Espacios
                        .FindAsync(ticket.EspacioId);

                    if (ticket.FechaSalida.HasValue && espacio != null)
                    {
                        espacio.Ocupado = false;
                    }

                    _context.Update(ticket);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // MODELSTATE INVÁLIDO
            // ========================================================

            ViewData["EspacioId"] = new SelectList(
                _context.Espacios,
                "EspacioId",
                "Codigo",
                ticket.EspacioId
            );

            ViewData["VehiculoId"] = new SelectList(
                _context.Vehiculos,
                "VehiculoId",
                "Placa",
                ticket.VehiculoId
            );

            return View(ticket);
        }

        // ============================================================
        // GET: Tickets/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // ============================================================
        // POST: Tickets/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket != null)
            {
                // Liberar espacio
                var espacio = await _context.Espacios
                    .FindAsync(ticket.EspacioId);

                if (espacio != null)
                {
                    espacio.Ocupado = false;
                }

                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // MÉTODO AUXILIAR PARA CARGAR COMBOS
        // ============================================================
        private void CargarCombos(
            int? vehiculoSeleccionado = null,
            int? espacioSeleccionado = null)
        {
            // --------------------------------------------------------
            // Espacios libres
            // --------------------------------------------------------

            var espaciosLibres = _context.Espacios
                .Where(e => !e.Ocupado)
                .OrderBy(e => e.Codigo)
                .ToList();

            ViewData["EspacioId"] = new SelectList(
                espaciosLibres,
                "EspacioId",
                "Codigo",
                espacioSeleccionado
            );

            // --------------------------------------------------------
            // Vehículos que NO tienen ticket activo
            // --------------------------------------------------------

            var vehiculosUsados = _context.Tickets
                .Where(t => t.Estado == "Activo")
                .Select(t => t.VehiculoId)
                .ToList();

            var vehiculosLibres = _context.Vehiculos
                .Where(v => !vehiculosUsados.Contains(v.VehiculoId))
                .OrderBy(v => v.Placa)
                .ToList();

            ViewData["VehiculoId"] = new SelectList(
                vehiculosLibres,
                "VehiculoId",
                "Placa",
                vehiculoSeleccionado
            );
        }

        // ============================================================
        // VERIFICAR EXISTENCIA
        // ============================================================
        private bool TicketExists(int id)
        {
            return _context.Tickets
                .Any(e => e.TicketId == id);
        }
    }
}