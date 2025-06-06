using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class AttendanceController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "attendance";
        return View();
    }
}
