using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IAttendanceService
{
    Task<Attendance?> GetAttendanceByDateAndUser(DateOnly date, int userId);
    Task<List<User>?> GetMyTeam(User user);
    Task<UserMonthlyAttendanceVM?> GetUserMonthlyAttendance(int month, int year,int userId);
    Task<Response> SaveAttendances(UserMonthlyAttendanceVM model);
}
