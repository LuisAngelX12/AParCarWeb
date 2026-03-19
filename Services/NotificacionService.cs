namespace AParCarWeb.Services
{
    using AParCarWeb.Data;
    using AParCarWeb.Models;

    public class NotificacionService
    {
        private readonly ApplicationDbContext _context;

        public NotificacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Crear(string mensaje)
        {
            var notificacion = new Notificacion
            {
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow,
                Leida = false
            };

            _context.Notificaciones.Add(notificacion);
            _context.SaveChanges();
        }
    }
}