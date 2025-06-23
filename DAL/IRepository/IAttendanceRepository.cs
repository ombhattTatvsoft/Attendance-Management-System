using Entity.Models;

namespace DAL.IRepository;

public interface IAttendanceRepository : IBaseRepository<Attendance>
{
    void DeleteAttendance(Attendance attendance);
}
