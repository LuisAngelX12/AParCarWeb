// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using AParCarWeb.Data;
using AParCarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AParCarWeb.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ApplicationDbContext _context;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        private async Task RegistrarBitacora(string accion)
        {
            var usuarioId = await _context.Usuarios
                .Where(u => u.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(u => u.UsuarioId)
                .FirstOrDefaultAsync();

            if (usuarioId == 0)
                return; // Usuario no existe, no registrar

            var bitacora = new BitacoraAcceso
            {
                UsuarioId = usuarioId,
                Accion = accion,
                IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FechaHora = DateTime.UtcNow
            };

            _context.BitacoraAccesos.Add(bitacora);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await RegistrarBitacora("Cierre de sesión");

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
