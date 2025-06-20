using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IReportAnalyticsService
{
    Task<List<ReportAnalyticsVM>> GetReportAnalyticsAsync(List<User> team, DateOnly? startDate, DateOnly? endDate);

}
