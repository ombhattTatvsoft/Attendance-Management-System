using BLL.IService;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _service;
    public AttendanceController(IAttendanceService service)
    {
        _service = service;
    }
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["selected"] = "attendance";
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> GetAttendanceTable(int month, int year, int? id)
    {
        UserMonthlyAttendanceVM? model = await _service.GetUserMonthlyAttendance(month, year, id);
        if (model != null)
            return PartialView("_AttendanceTable", model);
        return Json(new { success = false});
    }
    // [HttpPost]
    // public async Task<IActionResult> SaveAttendances([FromBody] UserMonthlyAttendanceVM model)
    // {
    //     return Json(new { success = false});
    // }
}
