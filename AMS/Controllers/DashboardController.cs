using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "Dashboard";
        return View();
    }
}
