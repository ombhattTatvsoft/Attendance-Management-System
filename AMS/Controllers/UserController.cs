using BLL.IService;
using BLL.Service;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "user";
        List<User> users = await _userService.GetAllUsersAsync();
        return View(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> UpsertUser(int? id)
    {
        ViewData["selected"] = "user";
        UpsertUserVM model = new();
        if (id == null)
            return View(model);
        else
            model = await _userService.GetUserAsync((int)id);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpsertUser(UpsertUserVM model)
    {
        if (model.UserId != 0)
            ModelState.Remove("Password");
        if (ModelState.IsValid)
        {
            Response response = await _userService.SaveUserAsync(model);
            if (response.IsSuccess)
            {
                TempData["success"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["failed"] = response.Message;
                return View(model);
            }
        }
        ViewData["selected"] = "user";
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetManagers(int departmentId)
    {
        List<User> managers = await _userService.GetManagersByDepartment(departmentId);
        return Json(managers.Select(m => new
        {
            m.UserId,
            m.Name
        }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> DeleteUser(int id)
    {
        Response response = await _userService.DeleteUserAsync(id);
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
