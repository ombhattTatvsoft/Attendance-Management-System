using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class ReportAnalyticsController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "reportAnalytics";
        return View();
    }
}
