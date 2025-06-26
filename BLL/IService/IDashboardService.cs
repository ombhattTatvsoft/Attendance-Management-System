using Entity.ViewModels;

namespace BLL.IService;

public interface IDashboardService
{
    Task<AdminDashboard?> GetAdminDashboardDataAsync();
    Task<DaysByAttendance> GetDaysByAttendanceAsync(UserMonthlyAttendanceVM monthlyData);
    Task<UpsertUserVM> GetMyProfile();
}
