using System.Diagnostics;
using System.Threading.Tasks;
using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

public class HomeController : Controller
{
    private readonly IHomeService _service;
    private readonly IUserService _userService;

    public HomeController(IHomeService service, IUserService userService)
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
                string? link = _service.PrepareLink(userFound, HttpContext);
                string subject = "Password Reset Request";
                string body = $@"<div style='width:50%;'>
<div style='display: flex;justify-content:center;align-items: center;background-color: #167BBC;padding:20px;gap:5px;'>
    <h2 style='color: white;'>AMS</h2>
</div>
<div style='padding: 20px;'>
<p>Attendance Management System,</p>
<p>Please click <a href={link}>
here</a> for reset your account Password.</p>
<p>If you encounter any issues or have any question,Please do not hesitate to contact our support team</p>
<p><span style='color:orange'>Important Note:</span> For security reasons,the link will expire in 24 hours. If you did not request a password reset, Please ignore this email or contact our support team immediately.</p>
</div>
</div>";

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
        if (!response.IsSuccess)
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
    [ValidateAntiForgeryToken]
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
