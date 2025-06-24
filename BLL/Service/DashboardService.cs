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
            model.TotalEmployee =(await _userRepository.GetAllAsync(x=>!x.IsDeleted && x.Role.RoleName!=EnumHelper.RoleToDbString(RoleEnum.Admin))).Count;
            var targetDate = DateOnly.FromDateTime(DateTime.Now);
            model.PresentEmployee = (await _attendanceRepository.GetAllAsync(x => x.Date == targetDate && (x.Status==EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Present) || x.Status==EnumHelper.AttendanceToDbString(AttendanceStatusEnum.HalfPresent)) && !x.User.IsDeleted)).Count;
            model.AbsentEmployee = model.TotalEmployee - model.PresentEmployee;
            model.SeniorManagers = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.SeniorManager))).Count;
            model.Managers = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.Manager))).Count;
            model.Employees = (await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.Employee))).Count;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting admin dashboard data.");
            return null;
        }
    }

}
