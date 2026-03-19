using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AParCarWeb.Data;
using AParCarWeb.Models;

namespace AParCarWeb.Controllers
{
    public class TarifasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TarifasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tarifas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tarifas.ToListAsync());
        }

        // GET: Tarifas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifa = await _context.Tarifas.FindAsync(id);
            if (tarifa == null)
            {
                return NotFound();
            }
            return View(tarifa);
        }

        // POST: Tarifas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TarifaId,Descripcion,PrecioHora")] Tarifa tarifa)
        {
            if (id != tarifa.TarifaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarifa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarifaExists(tarifa.TarifaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tarifa);
        }

        private bool TarifaExists(int id)
        {
            return _context.Tarifas.Any(e => e.TarifaId == id);
        }
    }
}
