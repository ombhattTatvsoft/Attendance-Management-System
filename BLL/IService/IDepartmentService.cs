using Entity.Models;

namespace BLL.IService;

public interface IDepartmentService
{
    Task<List<Department>> GetAllDepartmentsAsync();

}
