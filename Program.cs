using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ProyectoCamisetas API",
        Version = "v1",
        Description = "API para consultar y administrar camisetas"
    });
});

// DbContext (PostgreSQL)
builder.Services.AddDbContext<ProyectoCamisetas.Data.AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options
        .UseNpgsql(cs, npgsql =>
        {
            // Forzar nombre en minúsculas para la tabla de historial de migraciones
            npgsql.MigrationsHistoryTable("__efmigrationshistory", "public");
        })
        .UseSnakeCaseNamingConvention();
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Repositorios
builder.Services.AddScoped<ProyectoCamisetas.Repository.IUserRepository, ProyectoCamisetas.Repository.EfUserRepository>();
builder.Services.AddScoped<ProyectoCamisetas.Repository.ICamisetasRepository, ProyectoCamisetas.Repository.EfCamisetasRepository>();

// Autenticación por cookies (solo para OWNER)
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/"; // catálogo público
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        // Cookies seguras en producción y evitar problemas de proxies/CDN
        options.Cookie.Name = "cdg_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

// Asegurar base y semilla del OWNER (desde appsettings o variables de entorno)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ProyectoCamisetas.Data.AppDbContext>();
        // Aplicar migraciones pendientes (crea la base y tablas)
        db.Database.Migrate();
    }

    var ownerUser = builder.Configuration["Owner:Usuario"] ?? Environment.GetEnvironmentVariable("OWNER_USER");
    var ownerEmail = builder.Configuration["Owner:Email"] ?? Environment.GetEnvironmentVariable("OWNER_EMAIL");
    var ownerPassword = builder.Configuration["Owner:Password"] ?? Environment.GetEnvironmentVariable("OWNER_PASSWORD");
    if (!string.IsNullOrWhiteSpace(ownerUser) && !string.IsNullOrWhiteSpace(ownerEmail) && !string.IsNullOrWhiteSpace(ownerPassword))
    {
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ProyectoCamisetas.Repository.IUserRepository>();
        if (repo is ProyectoCamisetas.Repository.EfUserRepository ef)
            await ef.SeedOrUpdateOwnerAsync(ownerUser!, ownerEmail!, ownerPassword!);
    }
}
catch { /* no-op seed */ }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProyectoCamisetas API v1");
        c.RoutePrefix = "swagger"; // /swagger
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
