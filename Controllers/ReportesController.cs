using AParCarWeb.Data;
using AParCarWeb.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AParCarWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // REGISTRAR REPORTE GENERADO
        // ============================
        private void RegistrarReporte(string tipo)
        {
            var reporte = new ReporteGenerado
            {
                TipoReporte = tipo,
                FechaGeneracion = DateTime.UtcNow,
                GeneradoPor = User.Identity?.Name ?? "Sistema"
            };

            _context.ReportesGenerados.Add(reporte);
            _context.SaveChanges();
        }

        // ============================
        // DASHBOARD DE REPORTES
        // ============================
        public IActionResult Index()
        {
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.TotalVehiculos = _context.Vehiculos.Count();
            ViewBag.EspaciosDisponibles = _context.Espacios.Count(e => !e.Ocupado);

            ViewBag.IngresosHoy = _context.Pagos
                .Where(p => p.FechaPago.Date == DateTime.Today)
                .Sum(p => (decimal?)p.Monto) ?? 0;

            ViewBag.VehiculosDentro = _context.Tickets
                .Count(t => t.FechaSalida == null);

            return View();
        }

        // ============================
        // CLIENTES CON VEHÍCULOS
        // ============================
        public IActionResult ClientesVehiculos()
        {
            var reporte = _context.ClienteVehiculos
                .Include(cv => cv.Cliente)
                .Include(cv => cv.Vehiculo)
                .Where(cv => cv.Activo)
                .ToList();

            RegistrarReporte("Clientes con Vehículos Activos");

            return View(reporte);
        }

        // ============================
        // HISTORIAL CLIENTE VEHÍCULO
        // ============================
        public IActionResult HistorialClienteVehiculo()
        {
            var historial = _context.ClienteVehiculos
                .Include(cv => cv.Cliente)
                .Include(cv => cv.Vehiculo)
                .OrderByDescending(cv => cv.FechaInicio)
                .ToList();

            RegistrarReporte("Historial Cliente-Vehículo");

            return View(historial);
        }

        // ============================
        // VEHÍCULOS CON MÁS CLIENTES
        // ============================
        public IActionResult VehiculosMasClientes()
        {
            var reporte = _context.ClienteVehiculos
                .GroupBy(cv => cv.VehiculoId)
                .Select(g => new
                {
                    VehiculoId = g.Key,
                    TotalClientes = g.Count()
                })
                .OrderByDescending(x => x.TotalClientes)
                .ToList();

            RegistrarReporte("Vehículos con más clientes");

            return View(reporte);
        }

        // ============================
        // CLIENTES SIN VEHÍCULO
        // ============================
        public IActionResult ClientesSinVehiculo()
        {
            var reporte = _context.Clientes
                .Where(c => !c.ClienteVehiculos!.Any(cv => cv.Activo))
                .ToList();

            RegistrarReporte("Clientes sin vehículo");

            return View(reporte);
        }

        // ============================
        // REPORTE DE INGRESOS
        // ============================
        public IActionResult Ingresos(DateTime? desde, DateTime? hasta)
        {
            var pagos = _context.Pagos
                .Include(p => p.Ticket!)
                .ThenInclude(t => t.Vehiculo)
                .AsQueryable();

            if (desde.HasValue)
            {
                var desdeUtc = DateTime.SpecifyKind(desde.Value.Date, DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = DateTime.SpecifyKind(hasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago <= hastaUtc);
            }

            var resultado = pagos.OrderByDescending(p => p.FechaPago).ToList();
            ViewBag.Total = resultado.Sum(p => p.Monto);

            // **Agrega estas líneas para que la vista tenga las fechas actuales**
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            RegistrarReporte("Reporte de Ingresos");

            return View(resultado);
        }

        // ============================
        // OCUPACIÓN DEL PARQUEO
        // ============================
        public IActionResult Ocupacion()
        {
            var totalEspacios = _context.Espacios.Count();
            var ocupados = _context.Espacios.Count(e => e.Ocupado);

            var porcentaje = totalEspacios == 0
                ? 0
                : (decimal)ocupados / totalEspacios * 100;

            ViewBag.Total = totalEspacios;
            ViewBag.Ocupados = ocupados;
            ViewBag.Disponibles = totalEspacios - ocupados;
            ViewBag.Porcentaje = porcentaje;

            RegistrarReporte("Reporte de Ocupación");

            return View();
        }

        // ============================
        // EXPORTAR A EXCEL
        // ============================
        public IActionResult ExportarIngresosExcel(DateTime? desde, DateTime? hasta)
        {
            // Consulta base
            var pagos = _context.Pagos
                .Include(p => p.Ticket!)
                .ThenInclude(t => t.Vehiculo)
                .AsQueryable();

            // Aplicar filtro de fechas si hay
            if (desde.HasValue)
            {
                var desdeUtc = DateTime.SpecifyKind(desde.Value.Date, DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = DateTime.SpecifyKind(hasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago <= hastaUtc);
            }

            var resultado = pagos.OrderByDescending(p => p.FechaPago).ToList();

            RegistrarReporte("Exportación Excel de Ingresos");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ingresos");

            // Cabecera
            ws.Cell(1, 1).Value = "Fecha";
            ws.Cell(1, 2).Value = "Vehículo";
            ws.Cell(1, 3).Value = "Monto";

            int fila = 2;
            foreach (var p in resultado)
            {
                ws.Cell(fila, 1).Value = p.FechaPago;
                ws.Cell(fila, 2).Value = p.Ticket!.Vehiculo!.Placa;
                ws.Cell(fila, 3).Value = p.Monto;
                fila++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ReporteIngresos.xlsx");
        }

        // ============================
        // EXPORTAR A PDF
        // ============================
        public IActionResult ExportarIngresosPDF(DateTime? desde, DateTime? hasta)
        {
            var pagos = _context.Pagos
                .Include(p => p.Ticket!)
                .ThenInclude(t => t.Vehiculo)
                .AsQueryable();

            // Aplicar filtro si hay fechas
            if (desde.HasValue)
            {
                var desdeUtc = DateTime.SpecifyKind(desde.Value.Date, DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = DateTime.SpecifyKind(hasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                pagos = pagos.Where(p => p.FechaPago <= hastaUtc);
            }

            var resultado = pagos.OrderByDescending(p => p.FechaPago).ToList();

            RegistrarReporte("Exportación PDF de Ingresos");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Reporte de Ingresos")
                            .FontSize(18)
                            .Bold()
                            .AlignCenter()
                            .Underline();

                        // Mostrar rango de fechas
                        string rango = desde.HasValue || hasta.HasValue
                            ? $"Rango: {desde?.ToString("dd/MM/yyyy") ?? "∞"} - {hasta?.ToString("dd/MM/yyyy") ?? "∞"}"
                            : "Todos los registros";
                        col.Item().Text(rango).FontSize(10).AlignCenter();

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            // Columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120); // Fecha
                                columns.RelativeColumn();    // Vehículo
                                columns.ConstantColumn(80);  // Monto
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Fecha").SemiBold();
                                header.Cell().Element(CellStyle).Text("Vehículo").SemiBold();
                                header.Cell().Element(CellStyle).Text("Monto").SemiBold();
                            });

                            // Filas
                            foreach (var p in resultado)
                            {
                                table.Cell().Element(CellStyle).Text(p.FechaPago.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(CellStyle).Text(p.Ticket!.Vehiculo!.Placa);
                                table.Cell().Element(CellStyle).Text($"₡{p.Monto:N2}");
                            }

                            // Total al final
                            table.Cell().ColumnSpan(2).Element(CellStyle).Text("Total").Bold();
                            table.Cell().Element(CellStyle).Text($"₡{resultado.Sum(p => p.Monto):N2}").Bold();

                            // Estilo de celdas
                            static IContainer CellStyle(IContainer container)
                            {
                                return container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });
                    });
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);

            return File(stream.ToArray(), "application/pdf", "Ingresos.pdf");
        }
    }
}