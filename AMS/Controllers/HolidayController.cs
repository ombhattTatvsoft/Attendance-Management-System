using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class HolidayController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "holiday";
        return View();
    }
}
