using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IHolidayService
{
    Task<Response> DeleteHolidayAsync(int id);
    Task<Holiday?> GetHoliday(int id);
    Task<List<Holiday>> GetHolidays();
    Task<Response> SaveHolidayAsync(HolidayVM model);
}
