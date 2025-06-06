using System.Diagnostics;
using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomeService _service;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IHomeService service, IUserService userService)
    {
        _logger = logger;
        _service = service;
        _userService = userService;
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
    public async Task<IActionResult> Index(UserLoginVM user)
    {
        if (ModelState.IsValid)
        {
            User? userFound = await _service.AuthenticateUserAsync(user.Email.Trim(), user.Password);
            if (userFound == null)
            {
                TempData["failed"] = "Login Failed, Check Credentials!";
                return RedirectToAction("Index");
            }
            string token = _service.CreateJwtToken(userFound);
            _service.CreateCookie(userFound, HttpContext, token);
            TempData["success"] = "Login Successful";
            return RedirectToAction("Index", "Dashboard");
        }
        TempData["failed"] = "Login Failed";
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("JwtAMSToken");
        TempData["success"] = "Logout Successful";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVM user)
    {
        if (ModelState.IsValid)
        {
            User? userFound = await _userService.FindUserByEmailAsync(user.Email.Trim());
            if (userFound != null)
            {
                
            }
            else
            {
                TempData["failed"] = "User Not Found.";
                return View();
            }
        }
        else
        {
            TempData["failed"] = "Please Provide Email.";
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
