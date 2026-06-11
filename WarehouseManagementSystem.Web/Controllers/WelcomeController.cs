using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WarehouseManagementSystem.Web.Controllers
{
    [Route("")]
    public class WelcomeController : Controller
    {
        [AllowAnonymous]
        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
