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
        var aleatorias = await _camisetas.GetRandomAsync(12, onlyAvailable: true, ct);
        return View(aleatorias);
    }

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
