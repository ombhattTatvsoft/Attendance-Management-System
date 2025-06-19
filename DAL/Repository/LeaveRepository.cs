using DAL.IRepository;
using Entity.Models;

namespace DAL.Repository;

public class LeaveRepository : BaseRepository<Leave> ,ILeaveRepository
{
    private readonly AMSContext _context;

    public LeaveRepository(AMSContext context) : base(context)
    {
        _context = context;
    }
}
