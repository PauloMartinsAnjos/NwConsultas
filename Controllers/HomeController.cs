using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NwConsultas.Database;
using NwConsultas.Models;

namespace NwConsultas.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NwConsultasDbContext _context;

    public HomeController(ILogger<HomeController> logger, NwConsultasDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Estatísticas do dashboard
        var totalQueries = await _context.SavedQueries.CountAsync();
        var totalExecutions = await _context.QueryExecutions.CountAsync();
        var recentExecutions = await _context.QueryExecutions
            .OrderByDescending(e => e.ExecutedAt)
            .Take(10)
            .Include(e => e.SavedQuery)
            .ToListAsync();

        ViewBag.TotalQueries = totalQueries;
        ViewBag.TotalExecutions = totalExecutions;
        ViewBag.RecentExecutions = recentExecutions;

        return View();
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
