using AParCarWeb.Data;
using AParCarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class DashboardClienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardClienteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    IdentityUserId = identityUser.Id,
                    Nombre = identityUser.UserName ?? string.Empty,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            var cliente = await _context.Clientes
                .Include(c => c.ClienteVehiculos!)
                    .ThenInclude(cv => cv.Vehiculo)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    UsuarioId = usuario.UsuarioId,
                    Nombre = usuario.Nombre ?? string.Empty,
                    Identificacion = string.Empty,
                    Telefono = string.Empty,
                    TipoCliente = "Normal",
                    Email = identityUser.Email ?? string.Empty,
                    FechaRegistro = DateTime.UtcNow,
                    ClienteVehiculos = new List<ClienteVehiculo>()
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            cliente.ClienteVehiculos ??= new List<ClienteVehiculo>();

            ViewBag.TotalVehiculos = cliente.ClienteVehiculos.Count;

            var vehiculoIds = cliente.ClienteVehiculos
                .Select(cv => cv.VehiculoId)
                .ToList();

            var tickets = vehiculoIds.Any()
                ? await _context.Tickets
                    .Include(t => t.Pago)
                    .Where(t => vehiculoIds.Contains(t.VehiculoId))
                    .ToListAsync()
                : new List<Ticket>();

            ViewBag.TicketsActivos = tickets.Count(t => t.FechaSalida == null);
            ViewBag.TotalVisitas = tickets.Count;
            ViewBag.TotalPagado = tickets
                .Where(t => t.Pago != null && t.Pago.Estado == "Pagado")
                .Sum(t => t.Pago!.Monto);

            ViewBag.TotalPagado = tickets
                .Where(t => t.Pago != null && t.Pago.Estado == "Pagado")
                .Sum(t => t.Pago!.Monto);

            var pagosPendientes = tickets
                .Where(t => t.Pago == null || t.Pago.Estado == "Pendiente")
                .ToList();

            ViewBag.PagosPendientes = pagosPendientes;

            return View(cliente);
        }

        // ===================== ESPACIOS =====================
        public async Task<IActionResult> Espacios()
        {
            var espacios = await _context.Espacios
                .Include(e => e.Zona)
                .Where(e => !e.Ocupado)
                .ToListAsync();

            return View(espacios);
        }

        // ===================== RESERVAR =====================
        // GET: Mostrar pantalla para elegir vehículo y confirmar reserva
        public async Task<IActionResult> Reservar(int espacioId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            var cliente = await _context.Clientes
                .Include(c => c.ClienteVehiculos!)
                    .ThenInclude(cv => cv.Vehiculo)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario!.UsuarioId);

            if (cliente == null) return RedirectToAction("Index");

            // 🔹 FILTRO VEHÍCULOS DISPONIBLES
            var vehiculosDisponibles = new List<ClienteVehiculo>();
            foreach (var cv in cliente.ClienteVehiculos!.Where(cv => cv.Activo))
            {
                bool tieneTicketActivo = await _context.Tickets
                    .AnyAsync(t => t.VehiculoId == cv.VehiculoId && t.FechaSalida == null && t.Estado == "Activo");

                if (!tieneTicketActivo)
                {
                    vehiculosDisponibles.Add(cv);
                }
            }

            ViewBag.EspacioId = espacioId;
            return View(vehiculosDisponibles);
        }

        // GET: Mostrar confirmación
        public async Task<IActionResult> ConfirmarReserva(int vehiculoId, int espacioId)
        {
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.VehiculoId == vehiculoId);
            var espacio = await _context.Espacios.Include(e => e.Zona)
                             .FirstOrDefaultAsync(e => e.EspacioId == espacioId && !e.Ocupado);

            if (vehiculo == null || espacio == null)
                return RedirectToAction("Espacios");

            ViewBag.Vehiculo = vehiculo;
            ViewBag.Espacio = espacio;

            return View();
        }

        // POST: Crear ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarReservaPost(int vehiculoId, int espacioId)
        {
            var espacio = await _context.Espacios.FirstOrDefaultAsync(e => e.EspacioId == espacioId && !e.Ocupado);

            if (espacio == null)
            {
                TempData["Error"] = "El espacio ya está ocupado.";
                return RedirectToAction("Espacios");
            }

            var ticket = new Ticket
            {
                VehiculoId = vehiculoId,
                EspacioId = espacioId,
                FechaEntrada = DateTime.UtcNow,
                Estado = "Activo"
            };

            espacio.Ocupado = true;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ===================== HISTORIAL =====================
        public async Task<IActionResult> Historial()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            if (usuario == null)
                return RedirectToAction("Index");

            var cliente = await _context.Clientes
                .Include(c => c.ClienteVehiculos)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId);

            if (cliente == null)
                return RedirectToAction("Index");

            var vehiculoIds = cliente.ClienteVehiculos!
                .Select(cv => cv.VehiculoId)
                .ToList();

            var tickets = await _context.Tickets
                .Include(t => t.Vehiculo)
                .Include(t => t.Pago)
                .Include(t => t.Espacio!)
                    .ThenInclude(e => e.Zona)
                .Where(t => vehiculoIds.Contains(t.VehiculoId))
                .OrderByDescending(t => t.FechaEntrada)
                .ToListAsync();

            return View(tickets);
        }

        // ===================== PAGAR =====================
        public async Task<IActionResult> Pagar(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Vehiculo)
                .Include(t => t.Espacio)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Pago != null)
                return RedirectToAction("Historial");

            return View(ticket);
        }

        // GET: DashboardCliente/CrearVehiculo
        public IActionResult CrearVehiculo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearVehiculo(Vehiculo vehiculo)
        {
            if (string.IsNullOrWhiteSpace(vehiculo.Placa))
            {
                ModelState.AddModelError("Placa", "La placa es obligatoria");
            }

            if (string.IsNullOrWhiteSpace(vehiculo.Marca))
            {
                ModelState.AddModelError("Marca", "La marca es obligatoria");
            }

            if (string.IsNullOrWhiteSpace(vehiculo.Modelo))
            {
                ModelState.AddModelError("Modelo", "El modelo es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(vehiculo.Color))
            {
                ModelState.AddModelError("Color", "Debe seleccionar un color");
            }

            if (ModelState.ErrorCount > 2)
            {
                return View(vehiculo);
            }

            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario!.UsuarioId);

            if (cliente == null)
                return RedirectToAction("Vehiculos");

            // FECHA EN UTC
            vehiculo.FechaCreacion = DateTime.UtcNow;

            // Guardar Vehículo
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            // Relación Cliente-Vehículo
            var clienteVehiculo = new ClienteVehiculo
            {
                ClienteId = cliente.ClienteId,
                VehiculoId = vehiculo.VehiculoId,
                Activo = true
            };

            _context.ClienteVehiculos.Add(clienteVehiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Vehiculos");
        }

        // GET: DashboardCliente/Vehiculos
        public async Task<IActionResult> Vehiculos()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            if (usuario == null)
                return RedirectToAction("Index");

            var cliente = await _context.Clientes
                .Include(c => c.ClienteVehiculos!)
                    .ThenInclude(cv => cv.Vehiculo)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId);

            if (cliente == null)
                return RedirectToAction("Index");

            var vehiculos = cliente.ClienteVehiculos!.Select(cv => cv.Vehiculo).ToList();

            return View(cliente);
        }

        // GET: DashboardCliente/Pagos
        public async Task<IActionResult> Pagos()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Redirect("/Identity/Account/Login");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

            if (usuario == null)
                return RedirectToAction("Index");

            var cliente = await _context.Clientes
                .Include(c => c.ClienteVehiculos!)
                    .ThenInclude(cv => cv.Vehiculo)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId);

            if (cliente == null)
                return RedirectToAction("Index");

            var vehiculoIds = cliente.ClienteVehiculos!.Select(cv => cv.VehiculoId).ToList();

            var pagos = vehiculoIds.Any()
            ? await _context.Tickets
                .Include(t => t.Pago)
                .Where(t =>
                    vehiculoIds.Contains(t.VehiculoId) &&
                    t.Pago != null &&
                    t.Pago.Estado == "Pagado")
                .Select(t => t.Pago!)
                .ToListAsync()
            : new List<Pago>();

            return View(pagos);
        }

        [HttpPost]
        public async Task<IActionResult> SetDatosCliente(
            int UsuarioId,
            string Nombre,
            string Identificacion,
            string Telefono)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == UsuarioId);

            if (cliente == null)
                return RedirectToAction("Index");

            // 🔽 AQUÍ MISMO (ANTES DE GUARDAR)
            Identificacion = Identificacion.Replace("-", "").Trim();
            Telefono = Telefono.Replace("-", "").Trim();

            cliente.Nombre = Nombre;
            cliente.Identificacion = Identificacion;
            cliente.Telefono = Telefono;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CancelarTicket(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .Include(t => t.Pago)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null)
                return RedirectToAction("Historial");

            // ❌ No se puede cancelar si ya pagó
            if (ticket.Pago != null && ticket.Pago.Estado == "Pagado")
                return RedirectToAction("Historial");

            ticket.Estado = "Cancelado";
            ticket.FechaSalida = DateTime.UtcNow;
            ticket.Espacio!.Ocupado = false;

            if (ticket.Pago != null && ticket.Pago.Estado == "Pendiente")
            {
                ticket.Pago.Estado = "Cancelado";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Historial");
        }
    }
}
