using AParCarWeb.Data;
using AParCarWeb.Models;
using AParCarWeb.Models.ViewModels.Vmod1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClientesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================
        // LISTADO DE CLIENTES
        // =========================
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.Include(c => c.Usuario).ToListAsync();
            return View(clientes);
        }

        // =========================
        // DETALLES DE UN CLIENTE
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // =========================
        // EDITAR CLIENTE
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();

            // Obtener usuario Identity
            var user = await _userManager.FindByIdAsync(cliente.Usuario!.IdentityUserId!);

            var roles = await _userManager.GetRolesAsync(user!);

            var vm = new UsuarioEditVM
            {
                UsuarioId = cliente.UsuarioId,
                IdentityUserId = user!.Id,
                Nombre = cliente.Nombre,
                Email = user.Email,
                Rol = roles.FirstOrDefault() ?? "Cliente",
                EstadoUsuario = (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now) ? "Inactivo" : "Activo"
            };

            ViewBag.RolesList = new SelectList(new List<string> { "Admin", "Operador", "Cliente" }, vm.Rol);
            ViewBag.EstadosList = new SelectList(new List<string> { "Activo", "Inactivo" }, vm.EstadoUsuario);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioEditVM vm)
        {
            var cliente = await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();

            var user = await _userManager.FindByIdAsync(vm.IdentityUserId!);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                ViewBag.RolesList = new SelectList(new List<string> { "Admin", "Operador", "Cliente" }, vm.Rol);
                ViewBag.EstadosList = new SelectList(new List<string> { "Activo", "Inactivo" }, vm.EstadoUsuario);
                return View(vm);
            }

            // Actualizar usuario Identity
            user.Email = vm.Email;
            user.UserName = vm.Email;

            // Cambiar estado
            user.LockoutEnd = vm.EstadoUsuario == "Activo" ? null : DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);

            // Cambiar rol si es diferente
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(vm.Rol!))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!await _roleManager.RoleExistsAsync(vm.Rol!))
                    await _roleManager.CreateAsync(new IdentityRole(vm.Rol!));
                await _userManager.AddToRoleAsync(user, vm.Rol!);
            }

            // Actualizar cliente
            cliente.Nombre = vm.Nombre;
            _context.Update(cliente);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}