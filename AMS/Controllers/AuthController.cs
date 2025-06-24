using System.Diagnostics;
using System.Threading.Tasks;
using BLL.Helpers;
using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _service;

    private readonly IUserService _userService;

    public AuthController(IAuthService service, IUserService userService)
    {
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
            string? token = _service.CreateJwtToken(userFound);
            if (token == null)
            {
                TempData["failed"] = "Login Failed, Please Try Again!";
                return RedirectToAction("Index");
            }
            _service.CreateCookie(userFound, HttpContext, token);
            TempData["success"] = "Login Successful";
            return RedirectToAction("Index", "Dashboard");
        }
        TempData["failed"] = "Login Failed, Please Try Again!";
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
        if (Request.Cookies["JwtAMSToken"] != null)
        {
            TempData["success"] = "Login Successful";
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVM user)
    {
        if (ModelState.IsValid)
        {
            User? userFound = await _userService.FindUserByEmailAsync(user.Email.Trim());
            if (userFound != null)
            {
                string? link = _service.PrepareLink(userFound, HttpContext);
                string subject = EmailBodyHelper.ForgotPasswordSubject;
                string body = EmailBodyHelper.ForgotPasswordBody(link!);

                Response response = _service.SendEmail(userFound.Email, body, subject);
                if (response.IsSuccess)
                    TempData["success"] = response.Message;
                else
                    TempData["failed"] = response.Message;
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
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id)
    {
        Response response = await _service.CheckResetLinkAsync(id);
        if (!response.IsSuccess || string.IsNullOrEmpty(id))
        {
            TempData["failed"] = response.Message;
            return RedirectToAction("Index");
        }
        ResetPasswordVM model = new()
        {
            ResetCode = id
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordVM user)
    {
        if (ModelState.IsValid)
        {
            User? userFound = await _service.FindUserByResetCodeAsync(user.ResetCode);
            if (userFound != null)
            {
                await _userService.UpdatePasswordAsync(userFound, user.NewPassword);
                TempData["success"] = "Reset Password Successful";
                return RedirectToAction("index");
            }
        }
        TempData["failed"] = "Invalid Data";
        return RedirectToAction("index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
