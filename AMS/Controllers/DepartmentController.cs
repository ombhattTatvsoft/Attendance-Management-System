using BLL.IService;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _service;

    public DepartmentController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "department";
        List<Department> departments = await _service.GetAllDepartmentsAsync();
        return View(departments);
    }

    [HttpGet]
    public async Task<IActionResult> UpsertDepartment(int? id)
    {
        ViewData["selected"] = "department";
        UpsertDepartmentVM model = new();
        if (id != null)
            model = await _service.GetDepartmentAsync((int)id);
        return PartialView("_UpsertDepartment", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertDepartment(UpsertDepartmentVM model)
    {
        if (ModelState.IsValid)
        {
            Response response = await _service.SaveDepartmentAsync(model);
            if (response.IsSuccess)
            {
                TempData["Success"] = response.Message;
                return Json(new { success = true, message = response.Message });
            }
            else
            {
                TempData["failed"] = response.Message;
                return Json(new { success = false, message = response.Message });
            }
        }
        return Json(new { success = false, message = "Invalid data." });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        Response response = await _service.DeleteDepartmentAsync(id);
        if (response.IsSuccess)
        {
            TempData["success"] = response.Message;
        }
        else
        {
            TempData["failed"] = response.Message;
        }
        return RedirectToAction("Index");
    }
}
