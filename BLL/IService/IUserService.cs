using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IUserService
{
    Task<User?> FindUserByEmailAsync(string email);
    Task UpdatePasswordAsync(User user, string newPassword);
    Task<List<User>> GetAllUsersAsync();
    Task<UpsertUserVM> GetUserAsync(int id);
    Task<Response> SaveUserAsync(UpsertUserVM model);
    Task<List<User>> GetManagersByDepartmentAndRole(int departmentId, int roleId);
    Task<Response> DeleteUserAsync(int id);
    Task<User?> GetCurrentUserAsync();
    Task<List<User>> GetMyTeam(User user);
    Task<User?> GetUserById(int userId);
}
