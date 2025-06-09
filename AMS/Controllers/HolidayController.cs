using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class HolidayController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "holiday";
        return View();
    }
}
