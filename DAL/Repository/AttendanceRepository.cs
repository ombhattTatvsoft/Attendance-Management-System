using DAL.IRepository;
using Entity.Models;

namespace DAL.Repository;

public class AttendanceRepository :  BaseRepository<Attendance>, IAttendanceRepository
{
    private readonly AMSContext _context;

    public AttendanceRepository(AMSContext context) : base(context)
    {
        _context = context;
    }

    public void DeleteAttendance(Attendance attendance)
    {
        _context.Attendances.Remove(attendance);
    }

}
