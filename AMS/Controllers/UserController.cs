using BLL.Helpers;
using BLL.IService;
using BLL.Service;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly IAuthService _AuthService;

    private readonly IUserService _userService;
    
    private readonly IDepartmentService _departmentService;
    public UserController(IAuthService AuthService, IUserService userService, IDepartmentService departmentService)
    {
        _AuthService = AuthService;
        _userService = userService;
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["selected"] = "user";
        List<User> users = await _userService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> UpsertUser(int? id)
    {
        ViewData["selected"] = "user";
        UpsertUserVM model = new();
        if (id != null)
        {
            model = await _userService.GetUserAsync((int)id);
        }
        model.Departments = await _departmentService.GetAllDepartmentsAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertUser(UpsertUserVM model)
    {
        if (model.RoleId == 2)
        {
            ModelState.Remove("ManagerId");
        }
        if (ModelState.IsValid)
        {
            Response response = await _userService.SaveUserAsync(model);
            if (response.IsSuccess)
            {
                TempData["success"] = response.Message;
                if (model.UserId == 0)
                {
                    string? link = _AuthService.PrepareLink((User)response.Data!, HttpContext,true);
                    string subject = EmailBodyHelper.UpsertUserSubject;
                    string body = EmailBodyHelper.UpsertUserBody(link!);
                    Response emailResponse = _AuthService.SendEmail(model.Email, body, subject);
                    if (emailResponse.IsSuccess)
                        TempData["success"] = emailResponse.Message;
                    else
                        TempData["failed"] = emailResponse.Message;
                }
                return RedirectToAction("Index");
            }
            else
            {
                TempData["failed"] = response.Message;
                return View(model);
            }
        }
        ViewData["selected"] = "user";
        model.Departments = await _departmentService.GetAllDepartmentsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetManagers(int departmentId, int roleId)
    {
        List<User> managers = await _userService.GetManagersByDepartmentAndRole(departmentId, roleId);
        return Json(managers.Select(m => new
        {
            m.UserId,
            m.Name,
            m.Role.RoleName
        }));
    }

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
