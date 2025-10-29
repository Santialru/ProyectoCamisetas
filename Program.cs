using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

// ---------- Auth por cookies (ajustado para HTTP en ntempurl) ----------
builder.Services
  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(o =>
  {
      o.Cookie.Name = "cdg_auth";
      o.Cookie.HttpOnly = true;

      // En el subdominio temporal (HTTP) usá SameAsRequest.
      // Cuando tengas tu dominio con SSL: cambiá a CookieSecurePolicy.Always.
      o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
      o.Cookie.SameSite = SameSiteMode.Lax;

      o.LoginPath = "/User/Login";
      o.LogoutPath = "/User/Logout";
      o.AccessDeniedPath = "/";         // catálogo público
      o.SlidingExpiration = true;
      o.ExpireTimeSpan = TimeSpan.FromHours(8);
  });

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // idem observación de arriba
    o.Cookie.SameSite = SameSiteMode.Lax;
});

// ---------- MVC / Swagger ----------
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

// ---------- DbContext (PostgreSQL) ----------
builder.Services.AddDbContext<ProyectoCamisetas.Data.AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options
        .UseNpgsql(cs, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__efmigrationshistory", "public");
        })
        .UseSnakeCaseNamingConvention();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ---------- Repositorios ----------
builder.Services.AddScoped<ProyectoCamisetas.Repository.IUserRepository, ProyectoCamisetas.Repository.EfUserRepository>();
builder.Services.AddScoped<ProyectoCamisetas.Repository.ICamisetasRepository, ProyectoCamisetas.Repository.EfCamisetasRepository>();

var app = builder.Build();

// ---------- Migraciones + seed OWNER ----------
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ProyectoCamisetas.Data.AppDbContext>();
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
catch
{
    // no-op seed
}

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // IMPORTANTE: mientras uses el subdominio temporal SIN SSL, NO uses HSTS ni redirección HTTPS.
    // app.UseHsts();
}

// Swagger solo en Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProyectoCamisetas API v1");
        c.RoutePrefix = "swagger";
    });
}

// NO forzar HTTPS en ntempurl (rompe cookies Secure y la navegación)
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Anti-cache para HTML dinámico (y evitar 304 con ETag viejo)
app.Use(async (ctx, next) =>
{
    // Nunca devolver de caché del servidor
    ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0, private";
    ctx.Response.Headers["Pragma"] = "no-cache";
    ctx.Response.Headers["Expires"] = "0";
    ctx.Response.Headers["Vary"] = "Cookie";

    // No permitir validaciones condicionales que devuelvan 304 sobre HTML
    ctx.Request.Headers.Remove("If-None-Match");
    ctx.Response.Headers.Remove("ETag");

    await next();

    if ((ctx.Response.ContentType ?? "").StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0, private";
        ctx.Response.Headers.Remove("ETag");
    }
});


app.UseAuthentication();
app.UseAuthorization();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
