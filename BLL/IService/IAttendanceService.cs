using Entity.ViewModels;

namespace BLL.IService;

public interface IAttendanceService
{
    Task<UserMonthlyAttendanceVM?> GetUserMonthlyAttendance(int month, int year,int? id);

}
