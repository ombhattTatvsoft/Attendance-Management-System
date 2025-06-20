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
        if (user!.Role.RoleName == RoleHelper.ToDbString(RoleEnum.Admin))
            return RedirectToAction("Team");
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Team()
    {
        ViewData["selected"] = "leaveTeam";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UpsertLeave(int? id)
    {
        LeaveVM model = new();
        if (id != null)
            model = await _service.GetLeaveAsync((int)id);
        return PartialView("_UpsertLeave", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertLeave(LeaveVM model)
    {
        if (ModelState.IsValid)
        {
            Response response = await _service.UpsertLeaveAsync(model);
            if (response.IsSuccess)
                return Json(new { success = true, message = response.Message });
            else
                return Json(new { success = false, message = response.Message });
        }
        return Json(new { success = false, message = "Invalid data." });
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaves(DateOnly? startDate, DateOnly? endDate, string leaveStatus, int userId,bool? team)
    {
        List<LeaveVM> model = await _service.GetLeavesAsync(startDate, endDate, leaveStatus, userId,team);
        return PartialView("_LeaveTable", model);
    }

    [HttpPost]
    public async Task<IActionResult> CancelLeave(int id)
    {
        Response response = await _service.CancelLeaveAsync(id);
        if (response.IsSuccess)
            return Json(new { success = true, message = response.Message });
        else
            return Json(new { success = false, message = response.Message });
    }

    [HttpGet]
    public async Task<IActionResult> CountLeaveDays(DateOnly fromDate, DateOnly toDate, decimal fromDateType, decimal toDateType)
    {
        decimal days = await _service.CountLeaveDaysAsync(fromDate, toDate, fromDateType, toDateType);
        return Json(new { days });
    }

}
