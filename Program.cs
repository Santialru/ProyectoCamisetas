using System;
using System.IO;
using System.Net.Http;
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
builder.Services.AddMemoryCache(options => { options.SizeLimit = 1024; });
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

    // Dev-only: mirror missing /uploads files from a remote base URL and cache locally
    var remoteBase = builder.Configuration["Uploads:RemoteBaseUrl"];
    var enableMirror = (builder.Configuration["Uploads:EnableDevMirror"] ?? "true").Equals("true", StringComparison.OrdinalIgnoreCase);
    if (!string.IsNullOrWhiteSpace(remoteBase) && enableMirror)
    {
        var webRoot = app.Environment.WebRootPath ?? string.Empty;
        if (!webRoot.EndsWith(Path.DirectorySeparatorChar) && !webRoot.EndsWith(Path.AltDirectorySeparatorChar))
            webRoot += Path.DirectorySeparatorChar;
        var http = new HttpClient();

        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.HasValue && ctx.Request.Path.Value!.StartsWith("/uploads", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var rel = ctx.Request.Path.Value!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var localPath = Path.Combine(webRoot, rel);
                    if (!System.IO.File.Exists(localPath))
                    {
                        var remoteUrl = remoteBase!.TrimEnd('/') + ctx.Request.Path.Value;
                        using var resp = await http.GetAsync(remoteUrl, ctx.RequestAborted);
                        if (resp.IsSuccessStatusCode)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                            await using var fs = System.IO.File.Create(localPath);
                            await resp.Content.CopyToAsync(fs, ctx.RequestAborted);
                            // fall through so StaticFiles serves the newly cached file
                        }
                        else
                        {
                            // as a fallback, redirect so at least it displays in dev
                            ctx.Response.Redirect(remoteUrl, permanent: false);
                            return;
                        }
                    }
                }
                catch { /* ignore and let pipeline continue */ }
            }
            await next();
        });
    }
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
