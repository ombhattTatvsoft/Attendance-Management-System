using BLL.IService;
using DAL.IRepository;
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

    public async Task<List<ReportAnalyticsVM>> GetReportAnalyticsAsync(List<User> team, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            List<ReportAnalyticsVM> reportAnalytics = new();
            foreach (User user in team)
            {
                DateOnly newStartDate = startDate;
                newStartDate = DateOnly.FromDateTime(user.CreatedAt) > newStartDate ? DateOnly.FromDateTime(user.CreatedAt) : newStartDate;
                ReportAnalyticsVM report = new()
                {
                    User = user,
                    PresentCount = (await _attendanceRepository.GetAllAsync(x => x.Date >= newStartDate && x.Date <= endDate && x.UserId == user.UserId && x.Status == "Present")).Count,
                    HalfPresentCount = (await _attendanceRepository.GetAllAsync(x => x.Date >= newStartDate && x.Date <= endDate && x.UserId == user.UserId && x.Status == "Half Present")).Count
                };
                int holidayCount=0,satCount=0,sunCount=0;
                for (DateOnly date = newStartDate; date <= endDate; date = date.AddDays(1))
                {
                    if (await _holidayRepository.GetAsync(x => x.HolidayDate == date && !x.IsDeleted && x.HolidayDate.DayOfWeek != DayOfWeek.Saturday && x.HolidayDate.DayOfWeek != DayOfWeek.Sunday) != null)
                        holidayCount++;
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                        satCount++;
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                        sunCount++;
                }
                int dayDifference= (endDate.ToDateTime(TimeOnly.MinValue) - newStartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                report.AbsentCount = dayDifference - report.PresentCount - report.HalfPresentCount - holidayCount - satCount - sunCount;
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
