using System.Security.Claims;
using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserService _userService;

    private readonly IAuthService _AuthService;

    public DashboardController(IUserService userService,IAuthService AuthService)
    {
        _AuthService = AuthService;
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["selected"] = "dashboard";
        return View();
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        ViewData["selected"] = "dashboard";
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
    {
        ViewData["selected"] = "Dashboard";
        if (ModelState.IsValid)
        {
            if (model.CurrentPassword == model.NewPassword)
            {
                TempData["failed"] = "Current and New Password are same.";
                return View();
            }
            else
            {
                string email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value!;
                User? user = await _AuthService.AuthenticateUserAsync(email, model.CurrentPassword);
                if (user != null)
                {
                    await _userService.UpdatePasswordAsync(user, model.NewPassword);
                    TempData["success"] = "Password Changed Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["failed"] = "Password Change Failed";
                }
            }
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        string email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value!;
        User? user = await _userService.FindUserByEmailAsync(email);
        return PartialView("_UserData", user);
    }
}
