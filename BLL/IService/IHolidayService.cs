using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IHolidayService
{
    Task<List<Holiday>> GetHolidays();
    Task<Holiday?> GetHoliday(int id);
    Task<Response> SaveHolidayAsync(HolidayVM model);
    Task<Response> DeleteHolidayAsync(int id);
}
