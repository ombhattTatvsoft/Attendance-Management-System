using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
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
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            return await _repository.GetAllAsync(d => !d.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all departments.");
            return await Task.FromResult(new List<Department>());
        }
    }

}
