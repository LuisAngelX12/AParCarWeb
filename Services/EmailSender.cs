namespace AParCarWeb.Services
{
    using AParCarWeb.Models;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;

    public class EmailSender : ITemplatedEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly IRazorViewToStringRenderer _renderer;

        public EmailSender(IOptions<EmailSettings> settings, IRazorViewToStringRenderer renderer)
        {
            _settings = settings.Value;
            _renderer = renderer;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailRawAsync(email, subject, htmlMessage);
        }

        public async Task SendTemplateAsync<TModel>(string email, EmailTemplate template, TModel model)
        {
            var viewPath = template switch
            {
                EmailTemplate.ConfirmEmail => "/Templates/Email/ConfirmEmail.cshtml",
                EmailTemplate.ResetPassword => "/Templates/Email/ResetPassword.cshtml",
                EmailTemplate.ConfirmEmailChange => "/Templates/Email/ConfirmEmailChange.cshtml",
                EmailTemplate.WelcomeUser => "/Templates/Email/WelcomeUser.cshtml",
                EmailTemplate.PaymentSuccess => "/Templates/Email/PaymentSuccess.cshtml",
                _ => throw new ArgumentOutOfRangeException()
            };

            var html = await _renderer.RenderAsync(viewPath, model);

            var subject = template switch
            {
                EmailTemplate.ConfirmEmail => "Confirma tu cuenta",
                EmailTemplate.ResetPassword => "Restablecer contraseña",
                EmailTemplate.ConfirmEmailChange => "Confirmar cambio de email",
                EmailTemplate.WelcomeUser => "¡Bienvenido a AParCar Web!",
                EmailTemplate.PaymentSuccess => "Pago realizado con éxito",
                _ => ""
            };

            await SendEmailRawAsync(email, subject, html);
        }

        private async Task SendEmailRawAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.From!),
                Subject = subject,
                IsBodyHtml = true
            };

            message.To.Add(email);

            var htmlView = AlternateView.CreateAlternateViewFromString(
                htmlMessage, null, "text/html"
            );

            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "img",
                "LogoEmail.png"
            );

            if (File.Exists(logoPath))
            {
                var logo = new LinkedResource(logoPath, "image/png")
                {
                    ContentId = "logo_aparcar",
                    TransferEncoding = TransferEncoding.Base64
                };

                htmlView.LinkedResources.Add(logo);
            }

            message.AlternateViews.Add(htmlView);

            // SMTP
            var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}

