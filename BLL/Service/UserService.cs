using System.Security.Claims;
using BLL.IService;
using DAL.IRepository;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BLL.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task AddUserAsync(UpsertUserVM model)
    {
        User user = new()
        {
            Name = model.Name,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password, 13),
            RoleId = model.RoleId,
            ManagerId = model.ManagerId ?? null,
            Department = ((Department)model.Department).ToString(),
            CreatedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
        };
        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();
    }


    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync(
            includes: x =>x.Role
        );
    }


    public async Task<List<User>> GetManagersByDepartment(int departmentId)
    {
        return await _userRepository.GetAllAsync(u => u.Department == ((Department)departmentId).ToString() && u.Role.RoleName == "Manager");
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

    public Task UpdateUserAsync(UpsertUserVM model)
    {
        throw new NotImplementedException();
    }

}
