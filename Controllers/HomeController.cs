using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Data;
using ServiceCatalog.Web.Models;
using ServiceCatalog.Web.ViewModels;

namespace ServiceCatalog.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ServiceCatalogDbContext _context;

        public HomeController(ILogger<HomeController> logger, ServiceCatalogDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var viewModel = new HomeViewModel
            {
                Capabilities = await _context.Capabilities.OrderBy(c => c.Order).ToListAsync(),
                Locations = await _context.Locations.ToListAsync(),
                SelectedCategory = category ?? "All"
            };

            var servicesQuery = _context.Services
                .Include(s => s.Capability)
                .Include(s => s.Processes)
                .ThenInclude(p => p.Applicabilities) // CHANGED: Include applicabilities through processes
                .AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                servicesQuery = servicesQuery.Where(s => s.Capability.Name == category);
            }

            viewModel.Services = await servicesQuery.OrderBy(s => s.Order).ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> ServiceDetail(int id)
        {
            var service = await _context.Services
                .Include(s => s.Capability)
                .Include(s => s.Processes)
                .ThenInclude(p => p.Applicabilities) // CHANGED: Include applicabilities through processes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            var viewModel = new ServiceDetailViewModel
            {
                Service = service,
                Processes = service.Processes
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Order)
                    .ToList(),
                // CHANGED: Aggregate applicabilities from all processes
                Applicabilities = service.Processes
                    .SelectMany(p => p.Applicabilities)
                    .GroupBy(a => a.Division)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Any(a => a.IsApplicable) // True if ANY process is applicable for this division
                    )
            };

            return View(viewModel);
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
}
