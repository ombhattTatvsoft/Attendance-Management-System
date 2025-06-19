using Entity.ViewModels;

namespace BLL.IService;

public interface ILeaveService
{
    Task<decimal> CountLeaveDaysAsync(DateOnly fromDate, DateOnly toDate, decimal fromDateType, decimal toDateType);

    Task<LeaveVM> GetLeaveAsync(int id);
    Task<List<LeaveVM>> GetLeavesAsync(DateOnly? startDate, DateOnly? endDate, string leaveStatus,int userId);

    Task<Response> UpsertLeaveAsync(LeaveVM model);
}
