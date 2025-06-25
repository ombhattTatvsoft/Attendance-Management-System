using BLL.Helpers;
using BLL.IService;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize(Roles = "Admin,Senior Manager,Manager")]
public class ReportAnalyticsController : Controller
{
    private readonly IUserService _userService;
    private readonly IReportAnalyticsService _reportAnalyticsService;
    public ReportAnalyticsController(IUserService userService, IReportAnalyticsService reportAnalyticsService)
    {
        _userService = userService;
        _reportAnalyticsService = reportAnalyticsService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["selected"] = "reportAnalytics";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> FetchReport(DateOnly? startDate, DateOnly? endDate)
    {
        User? user = await _userService.GetCurrentUserAsync();
        List<User> team = await _userService.GetMyTeam(user!);
        if (user!.Role.RoleName != EnumHelper.RoleToDbString(RoleEnum.Admin))
        {
            team.Add(user);
        }
        List<ReportAnalyticsVM> model = await _reportAnalyticsService.GetReportAnalyticsAsync(team, startDate, endDate);
        return PartialView("_ReportTable",model);
    }
}
