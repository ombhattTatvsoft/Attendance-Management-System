using Entity.Models;
using Entity.ViewModels;

namespace BLL.IService;

public interface IUserService
{
    Task<User?> FindUserByEmailAsync(string email);
    Task UpdatePasswordAsync(User user, string newPassword);
    Task<List<User>> GetManagersByDepartment(int departmentId);
    Task AddUserAsync(UpsertUserVM model);
    Task UpdateUserAsync(UpsertUserVM model);
    Task<List<User>> GetAllUsersAsync();

}
