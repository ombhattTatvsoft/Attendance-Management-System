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
    public int UpsertedBy { get;  set; }
    public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public Task UpdatePasswordAsync(User user, string newPassword)
    {
        try
        {
            string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, 13);
            user.Password = passwordHash;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = UpsertedBy;
            _userRepository.Update(user);
            return _userRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", user.UserId);
            return null;
        }
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        try
        {
            return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding user by email: {Email}", email);
            return null;
        }
    }

    public async Task<Response> SaveUserAsync(UpsertUserVM model)
    {
        try
        {
            User? userWithEmail = await FindUserByEmailAsync(model.Email);
            User? user = (model.UserId == 0) ? new User() : await _userRepository.GetAsync(u => u.UserId == model.UserId && !u.IsDeleted);
            if (userWithEmail != null)
                if (model.UserId == 0 || (model.UserId != 0 && userWithEmail.UserId != model.UserId))
                    return new Response(false, "Email already exists");
            if (model.UserId != 0 && user == null)
                return new Response(false, "User not found");
            if (model.UserId == 0)
            {
                user = _mapper.Map<User>(model);
                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password, 13);
                user.CreatedBy = UpsertedBy;
                await _userRepository.AddAsync(user);
                return new Response(true, "User created successfully", user);
            }
            else
            {
                model.Password = user.Password;
                user = _mapper.Map(model, user);
                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = UpsertedBy;
                _userRepository.Update(user);
            }
            await _userRepository.SaveAsync();
            if (model.UserId == 0)
                return new Response(true, "User created successfully", user);
                return new Response(true, "User updated successfully", user);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user with email {Email}", model.Email);
            return new Response(false, "An error occurred while saving the user");
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            return await _userRepository.GetAllAsync(x => !x.IsDeleted, x => x.Role, x => x.Manager);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return new List<User>();
        }
    }

    public async Task<List<User>> GetManagersByDepartmentAndRole(int departmentId,int roleId)
    {
        try
        {
            return await _userRepository.GetAllAsync(u => u.Department == ((Department)departmentId).ToString() && !u.IsDeleted && u.RoleId<roleId,u=>u.Role);
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving managers for department {DepartmentId}", departmentId);
            return new List<User>();
        }
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
