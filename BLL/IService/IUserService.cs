using Entity.Models;

namespace BLL.IService;

public interface IUserService
{
    Task<User?> FindUserByEmailAsync(string email);
    Task UpdatePasswordAsync(User user, string newPassword);

}
