using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ReportAnalyticsController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "reportAnalytics";
        return View();
    }
}
