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

    private readonly IDepartmentService _departmentService;

    private readonly IAuthService _authService;

    public DashboardController(IUserService userService, IDashboardService service, IAttendanceService attendanceService, IDepartmentService departmentService, IAuthService authService)
    {
        _authService = authService;
        _userService = userService;
        _departmentService = departmentService;
        _attendanceService = attendanceService;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "dashboard";
        User? user = await _userService.GetCurrentUserAsync();
        List<User> team = await _userService.GetMyTeam(user!);
        bool hasTeam = team.Count > 0;
        TempData["hasTeam"] = hasTeam ? "true" : "false";
        UserMonthlyAttendanceVM? monthlyData = await _attendanceService.GetUserMonthlyAttendance(DateTime.Now.Month, DateTime.Now.Year, user!.UserId);
        DashboardVM model = new()
        {
            AdminDashboard = await _service.GetAdminDashboardDataAsync(),
            UserDashboard = await _service.GetDaysByAttendanceAsync(monthlyData!),
            Team = team
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetChartData(int month, int year, int userId)
    {
        ViewData["selected"] = "dashboard";
        UserMonthlyAttendanceVM? model = await _attendanceService.GetUserMonthlyAttendance(month, year, userId);
        DaysByAttendance daysByAttendance = await _service.GetDaysByAttendanceAsync(model!);
        var data = new
        {
            presentDays = daysByAttendance.PresentDays,
            fullAbsentDays = daysByAttendance.FullAbsentDays,
            halfDays = daysByAttendance.HalfDays,
            remainingDays = daysByAttendance.TotalDays - (daysByAttendance.PresentDays + daysByAttendance.FullAbsentDays + daysByAttendance.HalfDays),
            totalDays = daysByAttendance.TotalDays
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
        ViewData["selected"] = "dashboard";
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
                User? user = await _authService.AuthenticateUserAsync(email, model.CurrentPassword);
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
        User? user = await _userService.GetCurrentUserAsync();
        return PartialView("_UserData", user);
    }

    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        UpsertUserVM user = await _service.GetMyProfile();
        user.Departments = await _departmentService.GetAllDepartmentsAsync();
        return View(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> MyProfile(UpsertUserVM model)
    {
        if (model.RoleId == 2)
        {
            ModelState.Remove("ManagerId");
        }
        model.Departments = await _departmentService.GetAllDepartmentsAsync();
        if (ModelState.IsValid)
        {
            Response response = await _userService.SaveUserAsync(model);
            TempData[response.IsSuccess ? "success" : "failed"] = response.Message;
            if (response.IsSuccess)
                return RedirectToAction("MyProfile");
        }
        return View(model);
    }
}
