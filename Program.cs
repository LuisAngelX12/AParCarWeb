using AParCarWeb.Data;
using AParCarWeb.Models;
using AParCarWeb.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// ENV + DB
// ===============================
Env.Load();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DATABASE_URL' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===============================
// IDENTITY
// ===============================
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ===============================
// COOKIE / SESIÓN
// ===============================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.LoginPath = "/Identity/Account/Login";
});

builder.Services.AddControllersWithViews();

// Licencia para exportar en pdf
QuestPDF.Settings.License = LicenseType.Community;

// ===============================
// PAYPAL
// ===============================
builder.Services.Configure<PayPalSettings>(options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID")
        ?? throw new InvalidOperationException("PAYPAL_CLIENT_ID not found.");

    options.Secret = Environment.GetEnvironmentVariable("PAYPAL_SECRET")
        ?? throw new InvalidOperationException("PAYPAL_SECRET not found.");
});

// ===============================
// EMAIL
// ===============================
builder.Services.Configure<EmailSettings>(options =>
{
    options.From = Environment.GetEnvironmentVariable("EMAIL_FROM")
        ?? throw new InvalidOperationException("EMAIL_FROM not found.");

    options.SmtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER")
        ?? throw new InvalidOperationException("EMAIL_SMTP_SERVER not found.");

    if (!int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out var port))
    {
        throw new InvalidOperationException("EMAIL_PORT is invalid or not found.");
    }

    options.Port = port;

    options.Username = Environment.GetEnvironmentVariable("EMAIL_USERNAME")
        ?? throw new InvalidOperationException("EMAIL_USERNAME not found.");

    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
        ?? throw new InvalidOperationException("EMAIL_PASSWORD not found.");
});

// ===============================
// OTROS SERVICIOS
// ===============================
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddScoped<ITemplatedEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailSender>(sp => sp.GetRequiredService<ITemplatedEmailSender>());
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<ConfiguracionService>();

var app = builder.Build();

// ===============================
// PIPELINE
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ===============================
// SEED ROLES / USERS
// ===============================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Operador", "Cliente" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // =========================
    // ADMIN
    // =========================

    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
        ?? throw new Exception("ADMIN_EMAIL no está configurado.");

    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? throw new Exception("ADMIN_PASSWORD no está configurado.");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        await userManager.AddToRoleAsync(adminUser, "Admin");

    // =========================
    // OPERADOR
    // =========================

    var operadorEmail = Environment.GetEnvironmentVariable("OPERADOR_EMAIL")
        ?? throw new Exception("OPERADOR_EMAIL no está configurado.");

    var operadorPassword = Environment.GetEnvironmentVariable("OPERADOR_PASSWORD")
        ?? throw new Exception("OPERADOR_PASSWORD no está configurado.");

    var operadorUser = await userManager.FindByEmailAsync(operadorEmail);
    if (operadorUser == null)
    {
        operadorUser = new ApplicationUser
        {
            UserName = operadorEmail,
            Email = operadorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(operadorUser, operadorPassword);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    if (!await userManager.IsInRoleAsync(operadorUser, "Operador"))
        await userManager.AddToRoleAsync(operadorUser, "Operador");
}

// ===============================
// SEED CONFIGURACIONES / ADMIN
// ===============================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.ConfiguracionesSistema.Any())
    {
        context.ConfiguracionesSistema.AddRange(

            new ConfiguracionSistema
            {
                Clave = "TarifaHora",
                Valor = "800",
                Descripcion = "Tarifa por hora del parqueo"
            },

            new ConfiguracionSistema
            {
                Clave = "TiempoGraciaMinutos",
                Valor = "10",
                Descripcion = "Minutos de gracia antes de cobrar"
            },

            new ConfiguracionSistema
            {
                Clave = "TiempoMaximoEstadia",
                Valor = "720",
                Descripcion = "Tiempo máximo permitido en minutos"
            },

            new ConfiguracionSistema
            {
                Clave = "TiempoReservaMinutos",
                Valor = "30",
                Descripcion = "Duración de reserva de espacio"
            },

            new ConfiguracionSistema
            {
                Clave = "CapacidadMaxima",
                Valor = "50",
                Descripcion = "Capacidad total del parqueo"
            },

            new ConfiguracionSistema
            {
                Clave = "MultaExceso",
                Valor = "2000",
                Descripcion = "Multa por exceder el tiempo permitido"
            },

            new ConfiguracionSistema
            {
                Clave = "AlertarTiempoExcedido",
                Valor = "true",
                Descripcion = "Enviar alerta cuando se exceda el tiempo"
            }
        );

        context.SaveChanges();
    }
}

// =========================
// SEED TARIFAS
// =========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Tarifas.Any())
    {
        context.Tarifas.AddRange(

            new Tarifa
            {
                Descripcion = "Tarifa estándar",
                PrecioHora = 1000
            },

            new Tarifa
            {
                Descripcion = "Tarifa nocturna",
                PrecioHora = 800
            },

            new Tarifa
            {
                Descripcion = "Tarifa fin de semana",
                PrecioHora = 1200
            }

        );

        context.SaveChanges();
    }
}

// ===============================
// MIDDLEWARE FINAL
// ===============================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===============================
// ROUTES
// ===============================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
