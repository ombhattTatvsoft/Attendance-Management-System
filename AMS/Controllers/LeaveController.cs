using BLL.Helpers;
using BLL.IService;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class LeaveController : Controller
{
    private readonly ILeaveService _service;

    private readonly IUserService _userService;

    public LeaveController(ILeaveService service, IUserService userService)
    {
        _service = service;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "leaveSelf";
        User? user = await _userService.GetCurrentUserAsync();
        if (user!.Role.RoleName == EnumHelper.RoleToDbString(RoleEnum.Admin))
            return RedirectToAction("Team");
        return View(user);
    }

    [HttpGet]
    public IActionResult Team()
    {
        ViewData["selected"] = "leaveTeam";
        if (TempData.Peek("hasTeam").ToString() == "false")
            return RedirectToAction("Index");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UpsertLeave(int? id)
    {
        LeaveVM model = id != null ? await _service.GetLeaveAsync((int)id) : new();
        return PartialView("_UpsertLeave", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertLeave(LeaveVM model)
    {
        if (ModelState.IsValid)
        {
            Response response = await _service.UpsertLeaveAsync(model);
            return Json(new { success = response.IsSuccess, message = response.Message });
        }
        return Json(new { success = false, message = "Invalid data." });
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaves(DateOnly? startDate, DateOnly? endDate, string leaveStatus, int userId, bool? team)
    {
        List<LeaveVM> model = await _service.GetLeavesAsync(startDate, endDate, leaveStatus, userId, team);
        return PartialView("_LeaveTable", model);
    }

    [HttpPost]
    public async Task<IActionResult> CancelLeave(int id)
    {
        Response response = await _service.CancelLeaveAsync(id);
        return Json(new { success = response.IsSuccess, message = response.Message });
    }

    [HttpGet]
    public async Task<IActionResult> CountLeaveDays(DateOnly fromDate, DateOnly toDate, decimal fromDateType, decimal toDateType)
    {
        decimal days = await _service.CountLeaveDaysAsync(fromDate, toDate, fromDateType, toDateType);
        return Json(new { days });
    }

}
