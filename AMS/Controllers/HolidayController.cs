using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class HolidayController : Controller
{
    private readonly IHolidayService _service;
    public HolidayController(IHolidayService service)
    {
        _service = service;
    }
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["selected"] = "holiday";
        return View();
    }
    [HttpGet]
    public async Task<JsonResult> GetHolidays()
    {
        List<Holiday> holidays = await _service.GetHolidays();
        var data = holidays.Select(h => new
        {
            id = h.HolidayId,
            title = h.HolidayName,
            start = h.HolidayDate.ToString("yyyy-MM-dd")
        });
        return Json(data);
    }
    [HttpGet]
    public async Task<JsonResult> GetHoliday(int id)
    {
        Holiday? holiday = await _service.GetHoliday(id);
        if (holiday == null)
        {
            return Json(new { success = false, message = "Holiday not found." });
        }
        return Json(new { success = true, data = holiday });
    }
    [HttpPost]
    public async Task<JsonResult> SaveHoliday(HolidayVM model)
    {
        if (ModelState.IsValid)
        {
            Response response = await _service.SaveHolidayAsync(model);
            if (response.IsSuccess)
            {
                return Json(new { success = true, message = response.Message });
            }
            else
            {
                return Json(new { success = false, message = response.Message });
            }
        }
        return Json(new { success = false, message = "Invalid data." });
    }
    [HttpPost]
    public async Task<JsonResult> DeleteHoliday(int id)
    {
        Response response = await _service.DeleteHolidayAsync(id);
        if (response.IsSuccess)
        {
            return Json(new { success = true, message = response.Message });
        }
        else
        {
            return Json(new { success = false, message = response.Message });
        }
    }
}
