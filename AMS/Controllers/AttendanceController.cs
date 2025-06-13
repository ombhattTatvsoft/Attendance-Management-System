using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _service;
    private readonly IUserService _userService;
    public AttendanceController(IAttendanceService service, IUserService userService)
    {
        _service = service;
        _userService = userService;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "attendanceSelf";
        User? user = await _userService.GetCurrentUserAsync();
        if (user!.Role.RoleName == Entity.Enums.Role.Admin.ToString())
            return RedirectToAction("Team");
        return View(user);
    }
    [HttpGet]
    public async Task<IActionResult> Team()
    {
        ViewData["selected"] = "attendanceTeam";
        User? user = await _userService.GetCurrentUserAsync();
        List<User>? team = await _service.GetMyTeam(user!);
        return View(team);
    }
    [HttpGet]
    public async Task<IActionResult> GetAttendanceTable(int month, int year, int userId)
    {
        UserMonthlyAttendanceVM? model = await _service.GetUserMonthlyAttendance(month, year, userId);
        if (model != null)
            return PartialView("_AttendanceTable", model);
        return Json(new { success = false });
    }
    [HttpPost]
    public async Task<IActionResult> SaveAttendances([FromBody] UserMonthlyAttendanceVM model)
    {
        Response response = await _service.SaveAttendances(model);
        return Json(response);
    }
    [HttpGet]
    public async Task<IActionResult> Regularize()
    {
        ViewData["selected"] = "regularizeSelf";
        User? user = await _userService.GetCurrentUserAsync();
        if (user!.Role.RoleName == Entity.Enums.Role.Admin.ToString())
            return RedirectToAction("RegularizeTeam");
        return View(user);
    }
    [HttpGet]
    public async Task<IActionResult> RegularizeTeam()
    {
        ViewData["selected"] = "regularizeTeam";
        return View();
    }
}
