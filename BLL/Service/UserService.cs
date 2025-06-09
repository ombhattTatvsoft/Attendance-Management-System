using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
    }

    public Task UpdatePasswordAsync(User user, string newPassword)
    {
        try
        {
            string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, 13);
            user.Password = passwordHash;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = user.UserId;
            _userRepository.Update(user);
            return _userRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", user.UserId);
            throw new Exception("An error occurred while updating the password", ex);
        }
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<Response> SaveUserAsync(UpsertUserVM model)
    {
        try
        {
            User? userWithEmail = await FindUserByEmailAsync(model.Email);
            if (model.UserId == 0)
            {
                if (userWithEmail != null)
                {
                    return new Response(false, "Email already exists");
                }
                else
                {
                    model.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password, 13);
                    User user = _mapper.Map<User>(model);
                    user.CreatedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    await _userRepository.AddAsync(user);
                    await _userRepository.SaveAsync();
                    return new Response(true, "User created successfully", user);
                }
            }
            else
            {
                User? existingUser = await _userRepository.GetAsync(u => u.UserId == model.UserId);
                if (userWithEmail != null && userWithEmail.UserId != model.UserId)
                {
                    return new Response(false, "Email already exists");
                }
                if (existingUser == null)
                {
                    return new Response(false, "User not found");
                }
                existingUser = _mapper.Map(model, existingUser);
                existingUser.UpdatedAt = DateTime.Now;
                existingUser.UpdatedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _userRepository.Update(existingUser);
                await _userRepository.SaveAsync();
                return new Response(true, "User updated successfully", existingUser);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user with email {Email}", model.Email);
            return new Response(false, "An error occurred while saving the user");
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync(x => !x.IsDeleted, x => x.Role, x => x.Manager);
    }

    public async Task<List<User>> GetManagersByDepartment(int departmentId)
    {
        return await _userRepository.GetAllAsync(u => u.Department == ((Department)departmentId).ToString() && u.Role.RoleName == "Manager" && !u.IsDeleted);
    }

    public async Task<UpsertUserVM> GetUserAsync(int id)
    {
        try
        {
            User? user = await _userRepository.GetAsync(x => x.UserId == id && !x.IsDeleted);
            if (user == null)
                throw new Exception("User not found");

            UpsertUserVM model = _mapper.Map<UpsertUserVM>(user);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            throw new Exception("An error occurred while retrieving the user", ex);
        }
    }

    public async Task<Response> DeleteUserAsync(int id)
    {
        try
        {
            User? user = await _userRepository.GetAsync(x => x.UserId == id && !x.IsDeleted);
            if (user == null)
            {
                return new Response(false, "User not found");
            }
            if (await _userRepository.GetAsync(x => x.ManagerId == user.UserId && !x.IsDeleted) != null)
            {
                return new Response(false, "Cannot delete user who is a manager of other users");
            }
            user.IsDeleted = true;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return new Response(true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return new Response(false, "An error occurred while deleting the user");
        }
    }

}
