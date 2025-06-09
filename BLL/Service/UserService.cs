using System.Security.Claims;
using AutoMapper;
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
    private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
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

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<Response> SaveUserAsync(UpsertUserVM model)
    {
        if (model.UserId == 0)
        {
            if (await FindUserByEmailAsync(model.Email) != null)
            {
                return new Response(false, "Email already exists");
            }
            else
            {
                User user = new()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password, 13),
                    RoleId = model.RoleId,
                    ManagerId = model.ManagerId,
                    Department = ((Department)model.Department).ToString(),
                    CreatedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                };
                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();
                return new Response(true, "User created successfully", user);
            }
        }
        else
        {
            User? existingUser = await _userRepository.GetAsync(u => u.UserId == model.UserId);
            User? userWithEmail = await FindUserByEmailAsync(model.Email);
            if (userWithEmail != null && userWithEmail.UserId != model.UserId)
            {
                return new Response(false, "Email already exists");
            }
            if (existingUser == null)
            {
                return new Response(false, "User not found");
            }
            existingUser.Name = model.Name;
            existingUser.Email = model.Email;
            existingUser.RoleId = model.RoleId;
            existingUser.ManagerId = model.ManagerId;
            existingUser.Department = ((Department)model.Department).ToString();
            existingUser.UpdatedAt = DateTime.Now;
            existingUser.UpdatedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _userRepository.Update(existingUser);
            await _userRepository.SaveAsync();
            return new Response(true, "User updated successfully", existingUser);
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync(x=>!x.IsDeleted, x => x.Role, x => x.Manager);
    }

    public async Task<List<User>> GetManagersByDepartment(int departmentId)
    {
        return await _userRepository.GetAllAsync(u => u.Department == ((Department)departmentId).ToString() && u.Role.RoleName == "Manager" && !u.IsDeleted);
    }

    public async Task<UpsertUserVM> GetUserAsync(int id)
    {
        User? user = await _userRepository.GetAsync(x => x.UserId == id && !x.IsDeleted);
        if (user == null)
            throw new Exception("User not found");

        UpsertUserVM model = _mapper.Map<UpsertUserVM>(user);
        return model;
    }

    public async Task<Response> DeleteUserAsync(int id)
    {
        User? user =await _userRepository.GetAsync(x => x.UserId == id && !x.IsDeleted);
        if (user == null)
        {
            return new Response(false, "User not found");
        }
        if(await _userRepository.GetAsync(x => x.ManagerId == user.UserId && !x.IsDeleted)!=null)
        {
            return new Response(false, "Cannot delete user who is a manager of other users");
        }
        user.IsDeleted = true;
        _userRepository.Update(user);
        await _userRepository.SaveAsync();
        return new Response(true, "User deleted successfully");
    }

}
