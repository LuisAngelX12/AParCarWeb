using AParCarWeb.Data;
using AParCarWeb.Models;
using AParCarWeb.Models.ViewModels.Vmod2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AParCarWeb.Controllers
{
    [Authorize]
    public class VehiculosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiculosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO
        public async Task<IActionResult> Index()
        {
            var vehiculos = _context.Vehiculos
                .Include(v => v.ClienteVehiculos!)
                .ThenInclude(cv => cv.Cliente);

            return View(await vehiculos.ToListAsync());
        }

        // CREATE GET
        public IActionResult Create()
        {
            ViewBag.Clientes = _context.Clientes.ToList();
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehiculoVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = _context.Clientes.ToList();
                return View(model);
            }

            var vehiculo = new Vehiculo
            {
                Placa = model.Placa,
                Marca = model.Marca,
                Modelo = model.Modelo,
                Color = model.Color
            };

            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            foreach (var clienteId in model.ClientesSeleccionados)
            {
                _context.ClienteVehiculos.Add(new ClienteVehiculo
                {
                    ClienteId = clienteId,
                    VehiculoId = vehiculo.VehiculoId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.ClienteVehiculos)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);

            if (vehiculo == null) return NotFound();

            var vm = new VehiculoVM
            {
                VehiculoId = vehiculo.VehiculoId,
                Placa = vehiculo.Placa,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Color = vehiculo.Color,
                ClientesSeleccionados = vehiculo.ClienteVehiculos!
                    .Select(cv => cv.ClienteId)
                    .ToList()
            };

            // Lista de clientes para el select múltiple
            var clientes = _context.Clientes.ToList();
            ViewBag.ClientesList = new MultiSelectList(clientes, "ClienteId", "Nombre", vm.ClientesSeleccionados);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehiculoVM model)
        {
            // Validaciones manuales
            if (string.IsNullOrWhiteSpace(model.Placa))
                ModelState.AddModelError("Placa", "La placa es obligatoria");

            if (string.IsNullOrWhiteSpace(model.Marca))
                ModelState.AddModelError("Marca", "La marca es obligatoria");

            if (string.IsNullOrWhiteSpace(model.Modelo))
                ModelState.AddModelError("Modelo", "El modelo es obligatorio");

            if (string.IsNullOrWhiteSpace(model.Color))
                ModelState.AddModelError("Color", "Debe seleccionar un color");

            if (!ModelState.IsValid)
            {
                // Reponer la lista de clientes para el select múltiple
                var clientes = _context.Clientes.ToList();
                ViewBag.ClientesList = new MultiSelectList(clientes, "ClienteId", "Nombre", model.ClientesSeleccionados);

                return View(model);
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.ClienteVehiculos)
                .FirstOrDefaultAsync(v => v.VehiculoId == model.VehiculoId);

            if (vehiculo == null) return NotFound();

            vehiculo.Placa = model.Placa;
            vehiculo.Marca = model.Marca;
            vehiculo.Modelo = model.Modelo;
            vehiculo.Color = model.Color;

            // Limpiar relaciones anteriores
            _context.ClienteVehiculos.RemoveRange(vehiculo.ClienteVehiculos!);

            // Agregar nuevas relaciones
            foreach (var clienteId in model.ClientesSeleccionados)
            {
                _context.ClienteVehiculos.Add(new ClienteVehiculo
                {
                    ClienteId = clienteId,
                    VehiculoId = vehiculo.VehiculoId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // HISTORIAL
        public IActionResult Historial(int clienteId)
        {
            var historial = _context.ClienteVehiculos
                .Include(cv => cv.Vehiculo)
                .Where(cv => cv.ClienteId == clienteId)
                .OrderByDescending(cv => cv.FechaInicio)
                .ToList();

            return View(historial);
        }
    }
}

