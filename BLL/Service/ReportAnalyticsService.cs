using BLL.Helpers;
using BLL.IService;
using DAL.IRepository;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class ReportAnalyticsService : IReportAnalyticsService
{
    private readonly IAttendanceRepository _attendanceRepository;

    private readonly IHolidayRepository _holidayRepository;

    private readonly ILogger<ReportAnalyticsService> _logger;

    public ReportAnalyticsService(IAttendanceRepository attendanceRepository,IHolidayRepository holidayRepository,ILogger<ReportAnalyticsService> logger)
    {
        _attendanceRepository = attendanceRepository;
        _holidayRepository = holidayRepository;
        _logger = logger;
    }

    public async Task<List<ReportAnalyticsVM>> GetReportAnalyticsAsync(List<User> team, DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            List<ReportAnalyticsVM> reportAnalytics = new();
            DateOnly defaultEndDate = DateOnly.FromDateTime(DateTime.Now);
            foreach (User user in team)
            {
                DateOnly userCreatedAt = DateOnly.FromDateTime(user.CreatedAt);
                DateOnly newStartDate = startDate ?? userCreatedAt;
                DateOnly newEndDate = endDate ?? defaultEndDate;
                newStartDate = userCreatedAt > newStartDate ? userCreatedAt : newStartDate;
                if (userCreatedAt > newEndDate)
                    continue;
                List<Attendance> attendances = await _attendanceRepository.GetAllAsync(x => x.Date >= newStartDate && x.Date <= newEndDate && x.UserId == user.UserId);
                ReportAnalyticsVM report = new()
                {
                    User = user,
                    PresentCount = attendances.Count(x => x.Status == EnumHelper.AttendanceToDbString(AttendanceStatusEnum.Present)),
                    HalfPresentCount = attendances.Count(x => x.Status == EnumHelper.AttendanceToDbString(AttendanceStatusEnum.HalfPresent))
                };
                List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate >= newStartDate && x.HolidayDate <= newEndDate && !x.IsDeleted);
                int holidayCount = 0, satCount = 0, sunCount = 0;
                for (DateOnly date = newStartDate; date <= newEndDate; date = date.AddDays(1))
                {
                    if (holidays.Any(x => x.HolidayDate == date && x.HolidayDate.DayOfWeek != DayOfWeek.Saturday && x.HolidayDate.DayOfWeek != DayOfWeek.Sunday))
                        holidayCount++;
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                        satCount++;
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                        sunCount++;
                }
                int dayDifference = (newEndDate.ToDateTime(TimeOnly.MinValue) - newStartDate.ToDateTime(TimeOnly.MinValue)).Days + 1 - holidayCount - satCount - sunCount;
                report.AbsentCount = dayDifference - report.PresentCount - report.HalfPresentCount;
                report.AttendancePercentage = Math.Round((double)(report.PresentCount + (report.HalfPresentCount * 0.5)) / dayDifference * 100, 2);
                reportAnalytics.Add(report);
            }
            return reportAnalytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching report analytics");
            return new List<ReportAnalyticsVM>();
        }
    }

}
