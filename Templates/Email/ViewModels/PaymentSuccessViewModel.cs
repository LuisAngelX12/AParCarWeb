namespace AParCarWeb.Templates.Email.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public string? UserName { get; set; }
        public decimal? Amount { get; set; }
        public string? Fecha { get; set; }
        public string? Detalle { get; set; }
        public string? MetodoPago { get; set; }
        public string? Referencia { get; set; }
        public int? Horas { get; set; }
        public decimal? PrecioHora { get; set; }
        public decimal? Multa { get; set; } = 0;
    }
}
