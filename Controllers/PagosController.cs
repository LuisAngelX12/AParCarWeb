namespace AParCarWeb.Controllers
{
    using AParCarWeb.Data;
    using AParCarWeb.Helpers;
    using AParCarWeb.Models;
    using AParCarWeb.Services;
    using AParCarWeb.Templates.Email.ViewModels;
    using DocumentFormat.OpenXml.ExtendedProperties;
    using DocumentFormat.OpenXml.Spreadsheet;
    using DotNetEnv;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PayPalCheckoutSdk.Core;
    using PayPalCheckoutSdk.Orders;
    using System.Globalization;

    [Authorize]
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificacionService _notificacion;
        private readonly ConfiguracionService _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITemplatedEmailSender _emailSender;

        public PagosController(ApplicationDbContext context, NotificacionService notificacion, ConfiguracionService config, UserManager<ApplicationUser> userManager, ITemplatedEmailSender emailSender)
        {
            _context = context;
            _notificacion = notificacion;
            _config = config;
            _userManager = userManager;
            _emailSender = emailSender;
            Env.Load();
        }

        // ============================
        // INICIAR PAGO
        // ============================
        [HttpPost]
        public async Task<IActionResult> PagarPaypal(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Estado != "Activo")
                return BadRequest();

            var ahora = DateTime.UtcNow;

            // =========================
            // SELECCIONAR TARIFA
            // =========================
            Tarifa tarifa;

            if (ahora.DayOfWeek == DayOfWeek.Saturday || ahora.DayOfWeek == DayOfWeek.Sunday)
            {
                tarifa = await _context.Tarifas
                    .FirstAsync(t => t.Descripcion!.Contains("fin"));
            }
            else if (ahora.Hour >= 18 || ahora.Hour < 6)
            {
                tarifa = await _context.Tarifas
                    .FirstAsync(t => t.Descripcion!.Contains("nocturna"));
            }
            else
            {
                tarifa = await _context.Tarifas
                    .FirstAsync(t => t.Descripcion!.Contains("estándar"));
            }

            // =========================
            // CONFIGURACIONES
            // =========================
            var tiempoGracia = _config.ObtenerInt("TiempoGraciaMinutos", 10);
            var tiempoMax = _config.ObtenerInt("TiempoMaximoEstadia", 720);
            var multa = _config.ObtenerDecimal("MultaExceso", 2000);

            var minutos = (DateTime.UtcNow - ticket.FechaEntrada).TotalMinutes;

            decimal montoCRC;

            // =========================
            // TIEMPO DE GRACIA
            // =========================
            if (minutos <= tiempoGracia)
            {
                montoCRC = 0;
            }
            else
            {
                var horas = Math.Ceiling(minutos / 60);
                montoCRC = (decimal)horas * tarifa.PrecioHora;
            }

            // =========================
            // MULTA POR EXCESO
            // =========================
            if (minutos > tiempoMax)
            {
                montoCRC += multa;
            }

            // =========================
            // CONVERSIÓN A USD (PAYPAL)
            // =========================
            decimal tipoCambio = _config.ObtenerDecimal("TipoCambioUSD", 540);
            decimal montoUSD = Math.Round(montoCRC / tipoCambio, 2);

            if (montoUSD < 0.01m)
                montoUSD = 0.01m;

            // =========================
            // CREAR ORDEN PAYPAL
            // =========================
            var order = new OrdersCreateRequest();
            order.Prefer("return=representation");

            order.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        ReferenceId = ticket.TicketId.ToString(),
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "USD",
                            Value = montoUSD.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = Url.Action("Success", "Pagos", null, Request.Scheme),
                    CancelUrl = Url.Action("Cancel", "Pagos", null, Request.Scheme)
                }
            });

            var client = GetPayPalClient();
            var response = await client.Execute(order);
            var result = response.Result<Order>();

            return Redirect(result.Links.First(l => l.Rel == "approve").Href);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> PagarEfectivo(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Estado != "Activo")
                return BadRequest();

            var ahora = DateTime.UtcNow;
            var minutos = (ahora - ticket.FechaEntrada).TotalMinutes;

            var tiempoGracia = _config.ObtenerInt("TiempoGraciaMinutos", 10);
            var tiempoMax = _config.ObtenerInt("TiempoMaximoEstadia", 720);
            var multa = _config.ObtenerDecimal("MultaExceso", 2000);

            Tarifa tarifa;

            if (ahora.DayOfWeek == DayOfWeek.Saturday || ahora.DayOfWeek == DayOfWeek.Sunday)
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("fin"));
            else if (ahora.Hour >= 18 || ahora.Hour < 6)
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("nocturna"));
            else
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("estándar"));

            decimal montoCRC = 0;

            if (minutos > tiempoGracia)
            {
                var horas = (int)Math.Ceiling(minutos / 60);
                montoCRC = horas * tarifa.PrecioHora;
            }

            if (minutos > tiempoMax)
            {
                montoCRC += multa;
            }

            var pago = new Pago
            {
                TicketId = ticket.TicketId,
                TarifaId = tarifa.TarifaId,
                Monto = montoCRC,
                Estado = "Pagado",
                MetodoPago = "Efectivo",
                TransaccionId = Guid.NewGuid().ToString(),
                FechaPago = ahora
            };

            ticket.Estado = "Pagado";
            ticket.FechaSalida = ahora;
            ticket.Espacio!.Ocupado = false;

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            _notificacion.Crear($"Pago en efectivo ₡{montoCRC}");

            return RedirectToAction("Index", "Tickets", new { id = ticket.TicketId });
        }

        // ============================
        // CONFIRMAR PAGO EFECTIVO
        // ============================
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> ConfirmarEfectivo(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Estado != "Activo")
                return NotFound();

            var ahora = DateTime.UtcNow;
            var minutos = (ahora - ticket.FechaEntrada).TotalMinutes;

            var tiempoGracia = _config.ObtenerInt("TiempoGraciaMinutos", 10);
            var tiempoMax = _config.ObtenerInt("TiempoMaximoEstadia", 720);
            var multa = _config.ObtenerDecimal("MultaExceso", 2000);

            Tarifa tarifa;

            if (ahora.DayOfWeek == DayOfWeek.Saturday || ahora.DayOfWeek == DayOfWeek.Sunday)
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("fin"));
            else if (ahora.Hour >= 18 || ahora.Hour < 6)
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("nocturna"));
            else
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("estándar"));

            decimal monto = 0;
            int horas = 0;
            decimal multaAplicada = 0;

            if (minutos > tiempoGracia)
            {
                horas = (int)Math.Ceiling(minutos / 60);
                monto = horas * tarifa.PrecioHora;
            }

            if (minutos > tiempoMax)
            {
                monto += multa;
                multaAplicada = multa;
            }

            ViewBag.Monto = monto;
            ViewBag.Horas = horas;
            ViewBag.Multa = multaAplicada;
            ViewBag.Tarifa = tarifa.PrecioHora;

            return View(ticket);
        }

        // ============================
        // PAGO EXITOSO
        // ============================
        public async Task<IActionResult> Success(string token)
        {
            // Capturar la orden de PayPal
            var request = new OrdersCaptureRequest(token);
            request.RequestBody(new OrderActionRequest());

            var client = GetPayPalClient();
            var response = await client.Execute(request);
            var result = response.Result<Order>();

            if (result.Status != "COMPLETED")
                return BadRequest();

            // Obtener ticket
            var purchase = result.PurchaseUnits.First();
            int ticketId = int.Parse(purchase.ReferenceId);

            var ticket = await _context.Tickets
                .Include(t => t.Espacio)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null)
                return NotFound();

            // =========================
            // CALCULAR MONTO CORRECTO
            // =========================
            var ahora = DateTime.UtcNow;
            var minutos = (ahora - ticket.FechaEntrada).TotalMinutes;

            var tiempoGracia = _config.ObtenerInt("TiempoGraciaMinutos", 10);
            var tiempoMax = _config.ObtenerInt("TiempoMaximoEstadia", 720);
            var multa = _config.ObtenerDecimal("MultaExceso", 2000);

            // Determinar tarifa según horario
            Tarifa tarifa;
            if (ahora.DayOfWeek == DayOfWeek.Saturday || ahora.DayOfWeek == DayOfWeek.Sunday)
            {
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("fin"));
            }
            else if (ahora.Hour >= 18 || ahora.Hour < 6)
            {
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("nocturna"));
            }
            else
            {
                tarifa = await _context.Tarifas.FirstAsync(t => t.Descripcion!.Contains("estándar"));
            }

            decimal montoCRC;
            int horas = 0;
            decimal multaAplicada = 0;

            // Aplicar tiempo de gracia
            if (minutos <= tiempoGracia)
            {
                montoCRC = 0;
            }
            else
            {
                horas = (int)Math.Ceiling(minutos / 60);
                montoCRC = horas * tarifa.PrecioHora;
            }

            // Aplicar multa por exceso
            if (minutos > tiempoMax)
            {
                montoCRC += multa;
                multaAplicada = multa;
            }

            // =========================
            // GUARDAR PAGO
            // =========================
            var pago = new Pago
            {
                TicketId = ticketId,
                TarifaId = tarifa.TarifaId,
                Monto = montoCRC,
                Estado = "Pagado",
                MetodoPago = "PayPal",
                PaypalOrderId = result.Id,
                TransaccionId = result.Id,
                FechaPago = ahora
            };

            // Actualizar ticket
            ticket.Estado = "Pagado";
            ticket.FechaSalida = ahora;
            ticket.Espacio!.Ocupado = false;

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            _notificacion.Crear($"Pago registrado por ₡{pago.Monto}");

            var user = await _userManager.GetUserAsync(User);

            if (user?.Email is string email)
            {
                await _emailSender.SendTemplateAsync(
                    email,
                    EmailTemplate.PaymentSuccess,
                    new PaymentSuccessViewModel
                    {
                        UserName = user.UserName ?? "Usuario",
                        Amount = pago.Monto,
                        Fecha = TimeHelper.ToCostaRicaTime(pago.FechaPago).ToString("g"),
                        Detalle = $"Espacio: {ticket.Espacio?.Codigo ?? "N/A"} | Ticket #{ticket.TicketId}",
                        MetodoPago = pago.MetodoPago,
                        Referencia = pago.TransaccionId,
                        Horas = horas == 0 ? 1 : horas,
                        PrecioHora = tarifa.PrecioHora,
                        Multa = multaAplicada
                    });
            }

            return View();
        }

        // ============================
        // CANCELACIÓN
        // ============================
        public IActionResult Cancel()
        {
            // No se guarda nada, no se rompe nada
            return View();
        }

        // ============================
        // CLIENTE PAYPAL SANDBOX
        // ============================
        private PayPalHttpClient GetPayPalClient()
        {
            var clientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID");
            var secret = Environment.GetEnvironmentVariable("PAYPAL_SECRET");

            var env = new SandboxEnvironment(clientId, secret);
            return new PayPalHttpClient(env);
        }
    }
}