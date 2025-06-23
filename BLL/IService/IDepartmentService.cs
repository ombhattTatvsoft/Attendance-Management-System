using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IDepartmentService
{
    Task<Response> DeleteDepartmentAsync(int id);
    Task<List<Department>> GetAllDepartmentsAsync();
    Task<UpsertDepartmentVM> GetDepartmentAsync(int id);
    Task<Response> SaveDepartmentAsync(UpsertDepartmentVM model);

}
