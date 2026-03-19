using AParCarWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AParCarWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================
        // Módulo 1 – Usuarios y Seguridad + identity
        // =========================
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<BitacoraAcceso> BitacoraAccesos { get; set; }

        // =========================
        // Módulo 2 – Clientes y Vehículos
        // =========================
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<ClienteVehiculo> ClienteVehiculos { get; set; }

        // =========================
        // Módulo 3 – Espacios de Parqueo
        // =========================
        public DbSet<Zona> Zonas { get; set; }
        public DbSet<Espacio> Espacios { get; set; }

        // =========================
        // Módulo 4 – Entradas y Salidas
        // =========================
        public DbSet<Ticket> Tickets { get; set; }

        // =========================
        // Módulo 5 – Pagos y Tarifas
        // =========================
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        // =========================
        // Módulo 6 – Reportes y Configuración
        // =========================
        public DbSet<ConfiguracionSistema> ConfiguracionesSistema { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<ReporteGenerado> ReportesGenerados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // Usuario + identity
            // =========================
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.IdentityUser)
                .WithMany()
                .HasForeignKey(u => u.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // Cliente ↔ Vehiculo (N–N)
            // =========================
            modelBuilder.Entity<ClienteVehiculo>()
                .HasKey(cv => new { cv.ClienteId, cv.VehiculoId });

            modelBuilder.Entity<ClienteVehiculo>()
                .HasOne(cv => cv.Cliente)
                .WithMany(c => c.ClienteVehiculos)
                .HasForeignKey(cv => cv.ClienteId);

            modelBuilder.Entity<ClienteVehiculo>()
                .HasOne(cv => cv.Vehiculo)
                .WithMany(v => v.ClienteVehiculos)
                .HasForeignKey(cv => cv.VehiculoId);

            // =========================
            // Zona → Espacios (1–N)
            // =========================
            modelBuilder.Entity<Espacio>()
                .HasOne(e => e.Zona)
                .WithMany(z => z.Espacios)
                .HasForeignKey(e => e.ZonaId);

            // =========================
            // Ticket ↔ Pago (1–1)
            // =========================
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Pago)
                .WithOne(p => p.Ticket)
                .HasForeignKey<Pago>(p => p.TicketId);
        }
    }
}
