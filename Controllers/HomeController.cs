namespace AParCarWeb.Controllers
{
    using AParCarWeb.Data;
    using AParCarWeb.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Security.Principal;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data.ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, Data.ApplicationDbContext context, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            await CrearUsuarioSiNoExiste();

            string accionBitacora = null!;
            IActionResult resultado = null!;

            if (User.IsInRole("Admin"))
            {
                accionBitacora = "Acceso al Admin";
                resultado = RedirectToAction("Admin", "DashboardAdmin");
            }
            else if (User.IsInRole("Operador"))
            {
                accionBitacora = "Acceso al Operador";
                resultado = RedirectToAction("Index", "DashboardOperador");
            }
            else if (User.IsInRole("Cliente"))
            {
                accionBitacora = "Acceso al Cliente";
                resultado = RedirectToAction("Index", "DashboardCliente");
            }
            else
            {
                return Redirect("/Identity/Account/AccessDenied");
            }

            if (accionBitacora != null)
                await RegistrarBitacora(accionBitacora);

            return resultado;
        }

        [Authorize]
        public async Task<IActionResult> Aparcar()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Admin", "DashboardAdmin");
            }
            else if (User.IsInRole("Operador"))
            {
                return RedirectToAction("Index", "DashboardOperador");
            }
            else if (User.IsInRole("Cliente"))
            {
                return RedirectToAction("Index", "DashboardCliente");
            }

            return Redirect("/Identity/Account/AccessDenied");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task CrearUsuarioSiNoExiste()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (identityUserId == null)
                return;

            var existe = await _context.Usuarios
                .AnyAsync(u => u.IdentityUserId == identityUserId);

            if (!existe)
            {
                var usuario = new Usuario
                {
                    IdentityUserId = identityUserId,
                    Nombre = User!.Identity!.Name,
                    EstadoUsuario = "Activo",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }
        }

        private async Task RegistrarBitacora(string accion)
        {
            var usuarioId = await _context.Usuarios
                .Where(u => u.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(u => u.UsuarioId)
                .FirstOrDefaultAsync();

            if (usuarioId == 0)
                return; // Usuario no existe, no registrar

            var bitacora = new BitacoraAcceso
            {
                UsuarioId = usuarioId,
                Accion = accion,
                IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FechaHora = DateTime.UtcNow
            };

            _context.BitacoraAccesos.Add(bitacora);
            await _context.SaveChangesAsync();
        }
    }
}
