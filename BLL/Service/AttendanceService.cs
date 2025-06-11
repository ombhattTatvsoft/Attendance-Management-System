using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AttendanceService> _logger;
    private readonly IMapper _mapper;
    public int UpsertedBy { get; set; }
    public AttendanceService(IAttendanceRepository repository, IHolidayRepository holidayRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<AttendanceService> logger)
    {
        _repository = repository;
        _holidayRepository = holidayRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public async Task<UserMonthlyAttendanceVM?> GetUserMonthlyAttendance(int month, int year, int? id)
    {
        try
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            List<DateOnly> allDates = Enumerable.Range(1, daysInMonth)
                .Select(day => new DateOnly(year, month, day))
                .ToList();
            List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate.Month == month && x.HolidayDate.Year == year);
            List<DateOnly> holidayDates = holidays.Select(h => h.HolidayDate).ToList();
            UserMonthlyAttendanceVM model = new UserMonthlyAttendanceVM
            {
                UserId = id ?? UpsertedBy,
                Month = month,
                Year = year,
                AttendanceRecords = allDates.Select(date => new UserAttendanceVM
                {
                    Date = date,
                    Day = date.DayOfWeek.ToString(),
                    IsHoliday = holidayDates.Contains(date) || date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday,
                    Status = null,
                    IsApproved = null
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

}
