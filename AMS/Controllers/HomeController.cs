using System.Diagnostics;
using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomeService _service;

    public HomeController(ILogger<HomeController> logger, IHomeService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (Request.Cookies["JwtAMSToken"] != null)
        {
            TempData["success"] = "Login Successful";
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserLoginView user)
    {       
        if (ModelState.IsValid)
        {
            User? userfound = await _service.AuthenticateUser(user.Email.Trim(),user.Password);
            if(userfound == null){
                TempData["failed"] = "Login Failed, Check Credentials!";
                return RedirectToAction("Index");
            }
            string token = _service.CreateJwtToken(userfound);   
            _service.CreateCookie(userfound,HttpContext,token);
            TempData["success"] = "Login Successful";
            return RedirectToAction("Index","Dashboard");
        }
        TempData["failed"] = "Login Failed";
        return RedirectToAction("Index");
    }

    [HttpGet]   
    public IActionResult Logout()
    { 
        Response.Cookies.Delete("JwtAMSToken");
        TempData["success"] = "Logout Successful";
        return RedirectToAction("Index");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
