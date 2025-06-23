using Entity.ViewModels;

namespace BLL.IService;

public interface IDashboardService
{
    Task<AdminDashboard?> GetAdminDashboardDataAsync();

}
