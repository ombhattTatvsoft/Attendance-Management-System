using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class HolidayService : IHolidayService
{
    private readonly IHolidayRepository _repository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<UserService> _logger;

    private readonly IMapper _mapper;
    
    public int UpsertedBy { get; set; }
    
    public HolidayService(IHolidayRepository repository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<UserService> logger)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<Response> DeleteHolidayAsync(int id)
    {
        try
        {
            Holiday? holiday = await _repository.GetAsync(x => x.HolidayId == id && !x.IsDeleted);
            if (holiday == null)
            {
                return new Response(false, "Holiday not found.");
            }
            holiday.IsDeleted = true;
            _repository.Update(holiday);
            await _repository.SaveAsync();
            return new Response(true, "Holiday deleted successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting holiday with ID {Id}", id);
            return new Response(false, "An error occurred while deleting the holiday.");
        }
    }

    public async Task<Holiday?> GetHoliday(int id)
    {
        try
        {
            return await _repository.GetAsync(x => x.HolidayId == id && !x.IsDeleted);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching holiday with ID {Id}", id);
            return null;
        }
    }

    public async Task<List<Holiday>> GetHolidays()
    {
        try
        {
            return await _repository.GetAllAsync(x => !x.IsDeleted);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching holidays");
            return new List<Holiday>();
        }
    }

    public async Task<Response> SaveHolidayAsync(HolidayVM model)
    {
        try
        {
            Holiday? holiday = (model.HolidayId == 0) ? new Holiday() : await _repository.GetAsync(x => x.HolidayId == model.HolidayId && !x.IsDeleted);
            if (model.HolidayId == 0)
            {
                holiday= _mapper.Map<Holiday>(model);
                holiday.CreatedBy = UpsertedBy;
                holiday.UpdatedBy = UpsertedBy;
                await _repository.AddAsync(holiday);
            }
            else
            {   
                if(holiday == null)
                    return new Response(false, "Holiday not found.");
                if(model.HolidayDate <= DateOnly.FromDateTime(DateTime.Now))
                    return new Response(false, "Cannot update holiday to a past date.");
                holiday = _mapper.Map(model, holiday);
                holiday.UpdatedAt = DateTime.Now;
                holiday.UpdatedBy = UpsertedBy;
                _repository.Update(holiday);
            }
            await _repository.SaveAsync();
            return new Response(true, string.Format("Holiday {0} successfully.", model.HolidayId == 0 ? "added" : "updated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving holiday");
            return new Response(false, "An error occurred while saving the holiday.");
        }
    }
}
