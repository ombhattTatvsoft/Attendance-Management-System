using BLL.IService;
using DAL.IRepository;
using Entity.Models;

namespace BLL.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public Task UpdatePasswordAsync(User user, string newPassword)
    {
        string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, 13);
        user.Password = passwordHash;
        user.UpdatedAt = DateTime.Now;
        user.UpdatedBy = user.UserId;
        _userRepository.Update(user);
        return _userRepository.SaveAsync();
    }
}
