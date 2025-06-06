using BLL.IService;
using BLL.Service;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

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
    [HttpGet]
    public IActionResult UpsertUser()
    {
        ViewData["selected"] = "user";
        UpsertUserVM model = new();
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> UpsertUser(UpsertUserVM model)
    {
        if (ModelState.IsValid)
        {
            if (model.UserId == 0)
            {
                await _userService.AddUserAsync(model);
            }
            else
            {
                await _userService.UpdateUserAsync(model);
            }
            return RedirectToAction("Index");
        }
        ViewData["selected"] = "user";
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> GetManagers(int departmentId)
    {
        List<User> managers =await _userService.GetManagersByDepartment(departmentId);
        return Json(managers.Select(m => new
        {
            m.UserId,
            m.Name
        }));
    }
}
