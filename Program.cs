using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

// ---------- Autenticación por cookies ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "cdg_auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Cambiar a Always cuando tengas SSL real
        o.Cookie.SameSite = SameSiteMode.Lax;

        o.LoginPath = "/User/Login";
        o.LogoutPath = "/User/Logout";
        o.AccessDeniedPath = "/";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
    // En smarterasp (HTTP temporal) no usar HSTS ni redirección HTTPS.
    // app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProyectoCamisetas API v1");
        c.RoutePrefix = "swagger";
    });
}

// app.UseHttpsRedirection(); // desactivado mientras uses HTTP en ntempurl

app.UseStaticFiles();
app.UseRouting();

// ---------- Middleware anti-cache seguro ----------
app.Use(async (ctx, next) =>
{
    // Evitar interferir con Swagger
    if (ctx.Request.Path.StartsWithSegments("/swagger"))
    {
        await next();
        return;
    }

    ctx.Request.Headers.Remove("If-None-Match");

    ctx.Response.OnStarting(() =>
    {
        var ct = ctx.Response.ContentType ?? "";
        if (ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
        {
            var h = ctx.Response.Headers;
            h["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0, private";
            h["Pragma"] = "no-cache";
            h["Expires"] = "0";
            h["Vary"] = "Cookie";
            h.Remove("ETag");
        }
        return Task.CompletedTask;
    });

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
