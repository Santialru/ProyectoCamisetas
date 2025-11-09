using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoCamisetas.Models;
using ProyectoCamisetas.Repository;

namespace ProyectoCamisetas.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICamisetasRepository _camisetas;

    public HomeController(ILogger<HomeController> logger, ICamisetasRepository camisetas)
    {
        _logger = logger;
        _camisetas = camisetas;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var productos = await _camisetas.GetRandomAsync(5, onlyAvailable: false, ct);
        ViewBag.Destacada = await _camisetas.GetHomeFeaturedAsync(ct);
        ViewBag.DestacadasGrid = await _camisetas.GetHomeFeaturedGridAsync(ct);
        ViewBag.HeroSlides = await _camisetas.GetHomeCarouselSlidesAsync(ct);
        return View(productos);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
