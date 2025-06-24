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

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _repository;

    private readonly IUserService _userService;

    private readonly IAttendanceRepository _attendanceRepository;

    private readonly IHolidayRepository _holidayRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<LeaveService> _logger;

    private readonly IMapper _mapper;

    public int UpsertedBy { get; set; }

    public LeaveService(ILeaveRepository repository,IUserService userService, IAttendanceRepository attendanceRepository, IHolidayRepository holidayRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<LeaveService> logger)
    {
        _repository = repository;
        _attendanceRepository = attendanceRepository;
        _holidayRepository = holidayRepository;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public async Task<LeaveVM> GetLeaveAsync(int id)
    {
        try
        {
            Leave? leave = await _repository.GetAsync(x => x.LeaveId == id,x=>x.User);
            return leave == null ? new LeaveVM() : _mapper.Map<LeaveVM>(leave);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting leave with ID {Id}.", id);
            return new LeaveVM();
        }
    }

    public async Task<decimal> CountLeaveDaysAsync(DateOnly fromDate, DateOnly toDate, decimal fromDateType, decimal toDateType)
    {
        try
        {
            List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate >= fromDate && x.HolidayDate <= toDate && !x.IsDeleted);
            decimal totalDays = 0;
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !holidays.Any(h => h.HolidayDate == date))
                {
                    if (date == fromDate)
                        totalDays += fromDateType == 1 ? 1m : 0.5m;
                    else if (date == toDate)
                        totalDays += toDateType == 1 ? 1m : 0.5m;
                    else
                        totalDays += 1m;
                }
            }
            return totalDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while counting leave days from {StartDate} to {EndDate}.", fromDate, toDate);
            return 0;
        }
    }

    public async Task<Response> UpsertLeaveAsync(LeaveVM model)
    {
        try
        {
            
            Leave? leave = (model.LeaveId == 0) ? new Leave() : await _repository.GetAsync(x => x.LeaveId == model.LeaveId && !x.IsDeleted);
            if (model.LeaveId != 0 && leave == null)
                return new Response(false, "Leave not found");
            model.CreatedAt = leave.CreatedAt;
            leave = model.LeaveId == 0 ? _mapper.Map<Leave>(model) : _mapper.Map(model, leave);
            leave.UserId = model.UserId == 0 ? UpsertedBy : model.UserId;
            leave!.UpdatedBy = UpsertedBy;
            if (model.LeaveId == 0)
            {
                if (await _repository.GetAsync(x => x.UserId == leave.UserId && !x.IsDeleted &&
                ((x.FromDate >= leave.FromDate && x.FromDate <= leave.ToDate) || (x.ToDate <= leave.ToDate && x.ToDate >= leave.FromDate) || (x.FromDate <= leave.FromDate && x.ToDate >= leave.ToDate))) != null)
                    return new Response(false, "Leave already exists for this user in the selected date range.");
                leave.CreatedBy = UpsertedBy;
                await _repository.AddAsync(leave);
            }
            else
            {
                leave.ApprovedBy=model.IsApproved!=null && (bool)model.IsApproved?UpsertedBy:null;
                leave.UpdatedAt = DateTime.Now;
                _repository.Update(leave);
            }
            await _repository.SaveAsync();
            await DeleteAttendanceByLeaveIdAsync(leave.LeaveId);
            List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate >= leave.FromDate && x.HolidayDate <= leave.ToDate && !x.IsDeleted);
            for (var date = leave.FromDate; date <= leave.ToDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !holidays.Any(h => h.HolidayDate == date) && leave.IsApproved != false)
                {
                    Attendance attendance = new()
                    {
                        UserId = leave.UserId,
                        Date = date,
                        Status = EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Absent),
                        IsApproved = leave.IsApproved,
                        ApprovedBy = leave.IsApproved != null && (bool)leave.IsApproved ? UpsertedBy : null,
                        CreatedBy = UpsertedBy,
                        UpdatedBy = UpsertedBy,
                        LeaveId = leave.LeaveId
                    };
                    if(date == leave.FromDate)
                        attendance.Status = leave.FromDateLeaveDuration == 1 ? EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Absent) : EnumHelper.AttendanceToDbString(AttendanceStatusEnum.HalfPresent);
                    if(date == leave.ToDate)
                        attendance.Status = leave.ToDateLeaveDuration == 1 ? EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Absent) : EnumHelper.AttendanceToDbString(AttendanceStatusEnum.HalfPresent);

                    await _attendanceRepository.AddAsync(attendance);
                }
            }
            await _repository.SaveAsync();
            return new Response(true, string.Format("Leave {0} successfully", model.LeaveId == 0 ? "created" : "updated"), leave);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving Leave.");
            return new Response(false, "An error occurred while saving the Leave.");
        }
    }

    public async Task<List<LeaveVM>> GetLeavesAsync(DateOnly? startDate, DateOnly? endDate, string leaveStatus, int userId,bool? team)
    {
        try
        {
            User? user = await _userService.GetCurrentUserAsync();
            List<User>? userTeam = await _userService.GetMyTeam(user!);

            List<Leave> leaves = await _repository.GetAllAsync(x =>
                (startDate == null || x.FromDate >= startDate) &&
                (endDate == null || x.ToDate <= endDate) &&
                (leaveStatus == "All" || (leaveStatus == "Approved" && x.IsApproved == true) || (leaveStatus == "Pending" && x.IsApproved == null && !x.IsDeleted) || (leaveStatus == "Rejected" && x.IsApproved == false) || (leaveStatus == "Cancelled" && x.IsDeleted)), x => x.User, x => x.ApprovedByNavigation);
            if (team != null && (bool)team)
                leaves = leaves.Where(x => userTeam!.Any(u => u.UserId == x.UserId)).ToList();
            else
                leaves = leaves.Where(x => x.UserId == userId).ToList();
            return leaves.Select(leave => _mapper.Map<LeaveVM>(leave)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting leaves.");
            return new List<LeaveVM>();
        }
    }

    public async Task<Response> CancelLeaveAsync(int id)
    {
        try
        {
            Leave? leave = await _repository.GetAsync(x => x.LeaveId == id && !x.IsDeleted);
            if (leave == null)
                return new Response(false, "Leave not found");
            leave.IsDeleted = true;
            _repository.Update(leave);
            await DeleteAttendanceByLeaveIdAsync(leave.LeaveId);
            await _repository.SaveAsync();
            return new Response(true, "Leave cancelled successfully", leave);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling leave with ID {Id}.", id);
            return new Response(false, "An error occurred while cancelling the leave.");
        }
    }

    public async Task DeleteAttendanceByLeaveIdAsync(int leaveId)
    {
        try
        {
            List<Attendance> attendances = await _attendanceRepository.GetAllAsync(x => x.LeaveId == leaveId);
            foreach (Attendance attendance in attendances)
                _attendanceRepository.DeleteAttendance(attendance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting attendance for leave ID {LeaveId}.", leaveId);
        }
    }
}
