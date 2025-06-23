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

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;

    private readonly IUserRepository _userRepository;

    private readonly IHolidayRepository _holidayRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<AttendanceService> _logger;

    private readonly IMapper _mapper;

    public int UpsertedBy { get; set; }
    public AttendanceService(IAttendanceRepository repository, IUserRepository userRepository, IHolidayRepository holidayRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<AttendanceService> logger)
    {
        _repository = repository;
        _holidayRepository = holidayRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public async Task<UserMonthlyAttendanceVM?> GetUserMonthlyAttendance(int month, int year, int userId)
    {
        try
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            List<DateOnly> allDates = Enumerable.Range(1, daysInMonth)
                .Select(day => new DateOnly(year, month, day))
                .ToList();
            List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate.Month == month && x.HolidayDate.Year == year && !x.IsDeleted);
            List<DateOnly> holidayDates = holidays.Select(h => h.HolidayDate).ToList();
            List<Attendance> attendances = await _repository.GetAllAsync(x => x.UserId == userId && x.Date.Month == month && x.Date.Year == year);
            User? user = await _userRepository.GetAsync(x => x.UserId == userId);
            UserMonthlyAttendanceVM model = new()
            {
                UserId = userId,
                Month = month,
                Year = year,
                UserCreatedAt = user!.CreatedAt,
                AttendanceRecords = allDates.Select(date => new UserAttendanceVM
                {
                    Date = date,
                    Day = date.DayOfWeek.ToString(),
                    IsHoliday = holidayDates.Contains(date) || date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday,
                    Status = attendances.Where(x => x.Date == date).Select(x => x.Status).FirstOrDefault(),
                    IsApproved = attendances.Where(x => x.Date == date).Select(x => x.IsApproved).FirstOrDefault()
                }).ToList()
            };
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance table for user");
            return null;
        }
    }

    public async Task<Response> SaveAttendances(UserMonthlyAttendanceVM model)
    {
        try
        {
            foreach (UserAttendanceVM attendance in model.AttendanceRecords)
            {
                bool exist = true;
                Attendance? newAttendance = await _repository.GetAsync(x => x.Date == attendance.Date && x.UserId == attendance.UserId);
                if (newAttendance == null)
                {
                    exist = false;
                    newAttendance = new();
                }
                if (model.UserId != attendance.UserId)
                {
                    attendance.IsApproved = true;
                    newAttendance.ApprovedBy = model.UserId;
                }
                if (exist)
                {
                    newAttendance = _mapper.Map(attendance, newAttendance);
                    newAttendance.UpdatedAt = DateTime.Now;
                    newAttendance.UpdatedBy = UpsertedBy;
                    _repository.Update(newAttendance);
                }
                else
                {
                    newAttendance = _mapper.Map<Attendance>(attendance);
                    newAttendance.CreatedBy = UpsertedBy;
                    newAttendance.UpdatedBy = UpsertedBy;
                    await _repository.AddAsync(newAttendance);
                }
            }
            await _repository.SaveAsync();
            return new Response(true, "Attendance saved succesfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving attendance");
            return new Response(false, "Attendance is not saved");
        }
    }

    // public async Task<Attendance?> GetAttendanceByDateAndUser(DateOnly date, int userId)
    // {
    //     return await _repository.GetAsync(x=>x.Date==date && x.UserId==userId);
    // }
}
