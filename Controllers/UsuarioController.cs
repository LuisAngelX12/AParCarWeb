namespace AParCarWeb.Controllers
{
    using AParCarWeb.Data;
    using AParCarWeb.Models.ViewModels.Vmod1;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================
        // LISTAR USUARIOS
        // =========================
        public IActionResult Index()
        {
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        // =========================
        // EDIT - GET
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);
            if (usuario == null) return NotFound();

            var identityUser = await _userManager.FindByIdAsync(usuario.IdentityUserId!);
            var roles = await _userManager.GetRolesAsync(identityUser!);

            var vm = new UsuarioEditVM
            {
                UsuarioId = usuario.UsuarioId,
                IdentityUserId = usuario.IdentityUserId,
                Nombre = usuario.Nombre,
                Rol = roles.FirstOrDefault(),
                EstadoUsuario = usuario.EstadoUsuario
            };

            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(vm);
        }

        // =========================
        // EDIT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioEditVM model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.ToList();
                return View(model);
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == model.UsuarioId);
            if (usuario == null) return NotFound();

            var identityUser = await _userManager.FindByIdAsync(usuario.IdentityUserId!);

            // 1. Actualizar datos del sistema
            usuario.Nombre = model.Nombre;
            usuario.EstadoUsuario = model.EstadoUsuario!;

            // 2. Actualizar rol Identity
            var rolesActuales = await _userManager.GetRolesAsync(identityUser!);
            await _userManager.RemoveFromRolesAsync(identityUser!, rolesActuales);
            await _userManager.AddToRoleAsync(identityUser!, model.Rol!);

            // 3. Control de bloqueo
            if (model.EstadoUsuario == "Inactivo")
            {
                await _userManager.SetLockoutEndDateAsync(identityUser!, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(identityUser!, null);
            }

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Clientes");
        }

        public async Task<IActionResult> Bitacora()
        {
            // Incluye el usuario relacionado
            var bitacoras = await _context.BitacoraAccesos
                .Include(b => b.Usuario)
                .OrderByDescending(b => b.FechaHora)
                .ToListAsync();

            return View(bitacoras);
        }
    }
}
