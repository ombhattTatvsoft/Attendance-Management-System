using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IUserService
{
    Task UpdatePasswordAsync(User user, string newPassword);
    Task<User?> FindUserByEmailAsync(string email);
    Task<List<User>> GetManagersByDepartmentAndRole(int departmentId, int roleId);
    Task<Response> SaveUserAsync(UpsertUserVM model);
    Task<List<User>> GetAllUsersAsync();
    Task<UpsertUserVM> GetUserAsync(int id);
    Task<Response> DeleteUserAsync(int id);
    Task<User?> GetCurrentUserAsync();

}
