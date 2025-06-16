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
    private readonly IHomeService _homeService;
    private readonly IUserService _userService;
    public UserController(IHomeService homeService,IUserService userService)
    {
        _homeService = homeService;
        _userService = userService;
    }
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
        if (id == null)
            return View(model);
        else
            model = await _userService.GetUserAsync((int)id);
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
                    string? link = _homeService.PrepareLink((User)response.Data, HttpContext,true);
                    string subject = "Update Password Request";
                    string body = $@"
                <div style='width:50%;'>
                <div style='display: flex;justify-content:center;align-items: center;background-color: #167BBC;padding:20px;gap:5px;'>
                <h2 style='color: white;'>AMS</h2>
                </div>
                <div style='padding: 20px;'>
                <p>Welcome to Attendance Management System</p>
                <p>Please find the details below for login into your account.</p>
                <div style='border:2px solid;padding:5px;'>
                <p>Please click <a href={link}>
                here</a> and reset your password.</p>
                </div>
                <p>If you encounter any issues or have any question,Please do not hesitate to contact our support team</p>
                </div>
                </div>";
                    Response emailResponse = _homeService.SendEmail(model.Email, body, subject);
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
