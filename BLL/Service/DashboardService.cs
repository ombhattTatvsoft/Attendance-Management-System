using BLL.Helpers;
using BLL.IService;
using DAL.IRepository;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;

    private readonly IUserRepository _userRepository;
    
    private readonly IAttendanceRepository _attendanceRepository;

    public DashboardService(ILogger<DashboardService> logger, IUserRepository userRepository, IAttendanceRepository attendanceRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<AdminDashboard?> GetAdminDashboardDataAsync()
    {
        try
        {
            AdminDashboard model = new();
            model.TotalEmployee =(await _userRepository.GetAllAsync(x=>!x.IsDeleted && x.Role.RoleName!=RoleHelper.ToDbString(RoleEnum.Admin))).Count;
            model.PresentEmployee = (await _attendanceRepository.GetAllAsync(x => x.Date.ToDateTime(TimeOnly.MinValue) == DateTime.Now.Date && (x.Status=="Present" || x.Status=="Half Present") && !x.User.IsDeleted)).Count;
            model.AbsentEmployee = model.TotalEmployee - model.PresentEmployee;
            model.SeniorManagers = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==RoleHelper.ToDbString(RoleEnum.SeniorManager))).Count;
            model.Managers = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==RoleHelper.ToDbString(RoleEnum.Manager))).Count;
            model.Employees = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==RoleHelper.ToDbString(RoleEnum.Employee))).Count;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting admin dashboard data.");
            return null;
        }
    }

}
