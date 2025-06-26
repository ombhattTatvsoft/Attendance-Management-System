using System.Security.Claims;
using AutoMapper;
using BLL.Helpers;
using BLL.IService;
using DAL.IRepository;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;

    private readonly IUserRepository _userRepository;
    
    private readonly IAttendanceRepository _attendanceRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IMapper _mapper;

    public int UpsertedBy { get; set; }

    public DashboardService(ILogger<DashboardService> logger, IUserRepository userRepository, IAttendanceRepository attendanceRepository,IMapper mapper,IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        UpsertedBy = int.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<AdminDashboard?> GetAdminDashboardDataAsync()
    {
        try
        {
            AdminDashboard model = new();
            List<User> users = await _userRepository.GetAllAsync(x => !x.IsDeleted && x.Role.RoleName != EnumHelper.RoleToDbString(RoleEnum.Admin),x=>x.Role);
            model.TotalEmployee =users.Count;
            var targetDate = DateOnly.FromDateTime(DateTime.Now);
            model.PresentEmployee = (await _attendanceRepository.GetAllAsync(x => x.Date == targetDate && (x.StatusId==(int)AttendanceStatusEnum.Present || x.StatusId==(int)AttendanceStatusEnum.HalfPresent) && !x.User.IsDeleted)).Count;
            model.AbsentEmployee = model.TotalEmployee - model.PresentEmployee;
            model.SeniorManagers = users.Count(x=>x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.SeniorManager));
            model.Managers = users.Count(x=>x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.Manager));;
            model.Employees = users.Count(x=>x.Role.RoleName==EnumHelper.RoleToDbString(RoleEnum.Employee));;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting admin dashboard data.");
            return null;
        }
    }

    public async Task<DaysByAttendance> GetDaysByAttendanceAsync(UserMonthlyAttendanceVM monthlyData)
    {
        try
        {
            DaysByAttendance model = new()
            {
                FullAbsentDays = monthlyData.AttendanceRecords.Count(x => x.Date.ToDateTime(TimeOnly.MinValue) < DateTime.Now.Date && monthlyData.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue) && !(bool)x.IsHoliday && (x.StatusId == 0 || x.StatusId == (int)AttendanceStatusEnum.Absent))
                    + monthlyData.AttendanceRecords.Count(x => x.Date.ToDateTime(TimeOnly.MinValue) >= DateTime.Now.Date && x.StatusId == (int)AttendanceStatusEnum.Absent),
                HalfDays = monthlyData.AttendanceRecords.Count(x => x.StatusId == (int)AttendanceStatusEnum.HalfPresent),
                PresentDays = monthlyData.AttendanceRecords.Count(x => x.StatusId == (int)AttendanceStatusEnum.Present),
                TotalDays = monthlyData.AttendanceRecords.Count(x => !(bool)x.IsHoliday && monthlyData.UserCreatedAt!.Value.Date <= x.Date.ToDateTime(TimeOnly.MinValue)),
                Holidays = monthlyData.AttendanceRecords.Count(x => x.IsHoliday == true && x.Day != DayOfWeek.Saturday.ToString() && x.Day != DayOfWeek.Sunday.ToString())
            };
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting days by attendance data.");
            return new DaysByAttendance();
        }
    }

    public async Task<UpsertUserVM> GetMyProfile()
    {
        try
        {
            User? user = await _userRepository.GetAsync(x => !x.IsDeleted && x.UserId == UpsertedBy,x=>x.Manager);
            UpsertUserVM model = _mapper.Map<UpsertUserVM>(user);
            model.ManagerName = user.Manager?.Name;
            return model;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error occurred while getting profile with User ID {Id}.", UpsertedBy);
            return new UpsertUserVM();
        }
    }

}
