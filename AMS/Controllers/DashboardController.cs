using System.Security.Claims;
using BLL.Helpers;
using BLL.IService;
using Entity.Enums;
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
        User? user = await _userService.GetCurrentUserAsync();
        List<User> team = await _userService.GetMyTeam(user!);
        DashboardVM model = new()
        {
            AdminDashboard = await _service.GetAdminDashboardDataAsync(),
            UserDashboard = await _attendanceService.GetUserMonthlyAttendance(DateTime.Now.Month, DateTime.Now.Year, user!.UserId),
            Team = team
        };
        return View(model);
    }

        [HttpGet]
    public async Task<IActionResult> GetChartData(int month, int year, int userId)
    {
        ViewData["selected"] = "dashboard";
        UserMonthlyAttendanceVM? model = await _attendanceService.GetUserMonthlyAttendance(month, year, userId);
        
        double fullAbsentDays = model!.AttendanceRecords.Count(x => x.Date.ToDateTime(TimeOnly.MinValue) < DateTime.Now.Date && model.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue) && !(bool)x.IsHoliday && (x.Status == null || x.Status == EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Absent)))
        + model.AttendanceRecords.Count(x=>x.Date.ToDateTime(TimeOnly.MinValue)>=DateTime.Now.Date && x.Status==EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Absent));
        double halfDays = model.AttendanceRecords.Count(x => x.Status == EnumHelper.AttendanceToDbString(AttendanceStatusEnum.HalfPresent));
        double presentDays = model.AttendanceRecords.Count(x => x.Status == EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Present));
        double totalDays = model.AttendanceRecords.Count(x => !(bool)x.IsHoliday && model.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue));
        var data= new
        {
            presentDays,
            fullAbsentDays,
            halfDays,
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
                return View(model);
            }
            else
            {
                string email = HttpContext.User.FindFirstValue(ClaimTypes.Email)!;
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
        string email = HttpContext.User.FindFirstValue(ClaimTypes.Email)!;
        User? user = await _userService.FindUserByEmailAsync(email);
        return PartialView("_UserData", user);
    }
}
