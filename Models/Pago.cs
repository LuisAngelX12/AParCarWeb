namespace AParCarWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Pago
    {
        [Key]
        public int PagoId { get; set; }


        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }


        public int TarifaId { get; set; }
        public Tarifa? Tarifa { get; set; }


        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = "PayPal";
        public string Estado { get; set; } = "Pendiente";

        public string? PaypalOrderId { get; set; }
        public string? TransaccionId { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    }
}