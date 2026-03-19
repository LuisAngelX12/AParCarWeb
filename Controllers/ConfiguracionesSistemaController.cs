namespace AParCarWeb.Controllers
{
    using AParCarWeb.Data;
    using AParCarWeb.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin")]
    public class ConfiguracionesSistemaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionesSistemaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO
        public async Task<IActionResult> Index()
        {
            var configuraciones = await _context.ConfiguracionesSistema.ToListAsync();
            return View(configuraciones);
        }

        // EDITAR
        public async Task<IActionResult> Edit(int id)
        {
            var config = await _context.ConfiguracionesSistema.FindAsync(id);
            if (config == null)
                return NotFound();

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ConfiguracionSistema model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // CREAR
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfiguracionSistema model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.ConfiguracionesSistema.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        public async Task<IActionResult> Delete(int id)
        {
            var config = await _context.ConfiguracionesSistema.FindAsync(id);
            if (config == null)
                return NotFound();

            return View(config);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var config = await _context.ConfiguracionesSistema.FindAsync(id);
            _context.ConfiguracionesSistema.Remove(config!);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}