using AParCarWeb.Data;
using AParCarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Admin,Operador")]
    public class EspaciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EspaciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Espacios
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Espacios.Include(e => e.Zona);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Espacios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var espacio = await _context.Espacios
                .Include(e => e.Zona)
                .Include(e => e.Tickets!)
                    .ThenInclude(t => t.Vehiculo)
                .FirstOrDefaultAsync(m => m.EspacioId == id);

            if (espacio == null)
                return NotFound();

            // Buscar ticket activo (vehículo dentro)
            var ticketActivo = espacio.Tickets!
                .FirstOrDefault(t => t.FechaSalida == null);

            if (ticketActivo != null)
            {
                ViewBag.Vehiculo = ticketActivo.Vehiculo;
                ViewBag.FechaEntrada = ticketActivo.FechaEntrada;
            }

            return View(espacio);
        }

        // GET: Espacios/Create
        public IActionResult Create()
        {
            ViewData["ZonaId"] = new SelectList(_context.Zonas, "ZonaId", "Nombre");
            return View();
        }

        // POST: Espacios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EspacioId,ZonaId,Codigo,Ocupado")] Espacio espacio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(espacio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ZonaId"] = new SelectList(_context.Zonas, "ZonaId", "Nombre", espacio.ZonaId);
            return View(espacio);
        }

        // GET: Espacios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var espacio = await _context.Espacios
                .Include(e => e.Zona)
                .FirstOrDefaultAsync(e => e.EspacioId == id);

            if (espacio == null)
                return NotFound();

            // NO modificamos Ocupado aquí
            ViewData["ZonaId"] = new SelectList(_context.Zonas, "ZonaId", "Nombre", espacio.ZonaId);
            return View(espacio);
        }

        // POST: Espacios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EspacioId,ZonaId,Codigo")] Espacio espacio)
        {
            if (id != espacio.EspacioId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var espacioDb = await _context.Espacios.FindAsync(id);

                    if (espacioDb == null)
                        return NotFound();

                    // Solo actualizar campos permitidos
                    espacioDb.Codigo = espacio.Codigo;
                    espacioDb.ZonaId = espacio.ZonaId;

                    // NUNCA cambiar Ocupado
                    // espacioDb.Ocupado = ...   <-- Eliminado

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EspacioExists(espacio.EspacioId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ZonaId"] = new SelectList(_context.Zonas, "ZonaId", "Nombre", espacio.ZonaId);
            return View(espacio);
        }

        // GET: Espacios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var espacio = await _context.Espacios
                .Include(e => e.Zona)
                .FirstOrDefaultAsync(m => m.EspacioId == id);
            if (espacio == null)
            {
                return NotFound();
            }

            return View(espacio);
        }

        // POST: Espacios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var espacio = await _context.Espacios.FindAsync(id);
            if (espacio != null)
            {
                _context.Espacios.Remove(espacio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EspacioExists(int id)
        {
            return _context.Espacios.Any(e => e.EspacioId == id);
        }
    }
}
