using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<DepartmentService> _logger;

    private readonly IMapper _mapper;

    public int UpsertedBy { get; set; }

    public DepartmentService(IDepartmentRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<DepartmentService> logger)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            return await _repository.GetAllAsync(d => !d.IsDeleted, x => x.Users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all departments.");
            return new List<Department>();
        }
    }

    public async Task<UpsertDepartmentVM> GetDepartmentAsync(int id)
    {
        try
        {
            Department? department = await _repository.GetAsync(x => x.DepartmentId == id && !x.IsDeleted);
            return department == null ? new UpsertDepartmentVM() : _mapper.Map<UpsertDepartmentVM>(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting department with ID {Id}.", id);
            return new UpsertDepartmentVM();
        }
    }

    public async Task<Response> SaveDepartmentAsync(UpsertDepartmentVM model)
    {
        try
        {
            if (model.DepartmentId == 0)
            {
                Department department = _mapper.Map<Department>(model);
                department.CreatedBy = UpsertedBy;
                department.UpdatedBy = UpsertedBy;
                await _repository.AddAsync(department);
            }
            else
            {
                Department? existingDepartment = await _repository.GetAsync(x => x.DepartmentId == model.DepartmentId && !x.IsDeleted);
                if (existingDepartment == null)
                {
                    return new Response(false, "Department not found.");
                }
                Department department = _mapper.Map(model, existingDepartment);
                department.UpdatedAt = DateTime.Now;
                department.UpdatedBy = UpsertedBy;
                _repository.Update(department);
            }
            await _repository.SaveAsync();
            return new Response(true, string.Format("Department {0} successfully.", model.DepartmentId == 0 ? "created" : "updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving department.");
            return new Response(false, "An error occurred while saving the department.");
        }
    }

    public async Task<Response> DeleteDepartmentAsync(int id)
    {
        try
        {
            Department? department = await _repository.GetAsync(x => x.DepartmentId == id && !x.IsDeleted, x => x.Users);
            if (department == null)
            {
                return new Response(false, "Department not found");
            }
            if (department.Users.Any())
            {
                return new Response(false, "Cannot delete department with associated users");
            }
            department.IsDeleted = true;
            _repository.Update(department);
            await _repository.SaveAsync();
            return new Response(true, "Department deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department with ID {DepartmentId}", id);
            return new Response(false, "An error occurred while deleting the department");
        }
    }
}
