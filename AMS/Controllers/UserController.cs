using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class UserController : Controller
{
    public IActionResult Index()
    {
        ViewData["selected"] = "user";
        return View();
    }
}
