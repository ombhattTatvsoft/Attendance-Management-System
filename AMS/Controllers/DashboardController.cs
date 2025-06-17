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

    private readonly IDashboardService _service;

    private readonly IAttendanceService _attendanceService;

    private readonly IAuthService _AuthService;

    public DashboardController(IUserService userService, IDashboardService service, IAttendanceService attendanceService, IAuthService AuthService)
    {
        _AuthService = AuthService;
        _userService = userService;
        _attendanceService = attendanceService;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "dashboard";
        int userId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        User? user = await _userService.GetCurrentUserAsync();
        List<User> team = await _attendanceService.GetMyTeam(user!);
        DashboardVM model = new()
        {
            AdminDashboard = await _service.GetAdminDashboardDataAsync(),
            UserDashboard = await _attendanceService.GetUserMonthlyAttendance(DateTime.Now.Month, DateTime.Now.Year, userId),
            Team = team
        };
        return View(model);
    }

        [HttpGet]
    public async Task<IActionResult> GetChartData(int month, int year, int userId)
    {
        ViewData["selected"] = "dashboard";
        UserMonthlyAttendanceVM? model = await _attendanceService.GetUserMonthlyAttendance(month, year, userId);
        double fullAbsentDays = model!.AttendanceRecords.Where(x => x.Date.ToDateTime(TimeOnly.MinValue) <= DateTime.Now.Date && model.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue) && !(bool)x.IsHoliday && (x.Status == null || x.Status == "Absent")).Count();
        double halfDays = model.AttendanceRecords.Where(x => x.Status == "Half Present").Count() / 2.0;
        double presentDays = model.AttendanceRecords.Where(x => x.Status == "Present").Count() + halfDays;
        double totalDays = model.AttendanceRecords.Where(x => !(bool)x.IsHoliday && model.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue)).Count();
        var data= new
        {
            presentDays = presentDays,
            fullAbsentDays = fullAbsentDays,
            halfDays = halfDays,
            remainingDays = totalDays-(presentDays + fullAbsentDays + halfDays)
        };
        return Json(data);
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
