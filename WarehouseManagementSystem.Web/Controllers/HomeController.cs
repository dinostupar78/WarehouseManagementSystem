using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardRepository _dashboardRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DashboardRepository dashboardRepository, ILogger<HomeController> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dashboardRepository.GetDashboardAsync();
            return View(model);
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
