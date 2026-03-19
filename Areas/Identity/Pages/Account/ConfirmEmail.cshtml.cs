#nullable disable

using AParCarWeb.Data;
using AParCarWeb.Services;
using AParCarWeb.Templates.Email.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AParCarWeb.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITemplatedEmailSender _emailSender;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ITemplatedEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            // 1️⃣ Validar parámetros
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                StatusMessage = "Parámetros inválidos para confirmar el email.";
                return Page();
            }

            // 2️⃣ Buscar usuario
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = $"No se pudo cargar el usuario con ID '{userId}'.";
                return Page();
            }

            // 3️⃣ Decodificar token
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch
            {
                StatusMessage = "Token de confirmación inválido.";
                return Page();
            }

            // 4️⃣ Confirmar email
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                StatusMessage = "No se pudo confirmar tu email. Intenta nuevamente.";
                return Page();
            }

            // 5️⃣ Crear token de sesión para la app
            var sessionToken = Guid.NewGuid().ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "AppAuth", "Login", sessionToken);

            // 6️⃣ Iniciar sesión automáticamente
            await _signInManager.SignInAsync(user, isPersistent: false);

            await _emailSender.SendTemplateAsync(
                    user.Email,
                    EmailTemplate.WelcomeUser,
                    new WelcomeUserViewModel
                    {
                        UserName = user.UserName,
                        WelcomeMessage = "Gracias por registrarte en AParCar Web. ¡Disfruta de nuestra app!"
                    });

            StatusMessage = "¡Gracias por confirmar tu cuenta! Ahora te puedes loguear.";
            return Page();
        }
    }
}