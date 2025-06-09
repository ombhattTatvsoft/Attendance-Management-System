using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "attendance";
        return View();
    }
}
