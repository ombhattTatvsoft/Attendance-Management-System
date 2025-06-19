using System.Security.Claims;
using AutoMapper;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Service;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _repository;

    private readonly IHolidayRepository _holidayRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<LeaveService> _logger;

    private readonly IMapper _mapper;

    public int UpsertedBy { get; set; }

    public LeaveService(ILeaveRepository repository, IHolidayRepository holidayRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<LeaveService> logger)
    {
        _repository = repository;
        _holidayRepository = holidayRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _logger = logger;
        UpsertedBy = int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public async Task<LeaveVM> GetLeaveAsync(int id)
    {
        try
        {
            Leave? leave = await _repository.GetAsync(x => x.LeaveId == id && !x.IsDeleted);
            if (leave == null)
                return new LeaveVM();
            LeaveVM model = _mapper.Map<LeaveVM>(leave);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting leave with ID {Id}.", id);
            return new LeaveVM();
        }
    }

    public async Task<decimal> CountLeaveDaysAsync(DateOnly fromDate, DateOnly toDate, decimal fromDateType, decimal toDateType)
    {
        try
        {
            List<Holiday> holidays = await _holidayRepository.GetAllAsync(x => x.HolidayDate >= fromDate && x.HolidayDate <= toDate && !x.IsDeleted);
            decimal totalDays = 0;
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !holidays.Any(h => h.HolidayDate == date))
                {
                    if (date == fromDate)
                        totalDays += fromDateType == 1 ? 1m : 0.5m;
                    else if (date == toDate)
                        totalDays += toDateType == 1 ? 1m : 0.5m;
                    else
                        totalDays += 1m;
                }
            }
            return totalDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while counting leave days from {StartDate} to {EndDate}.", fromDate, toDate);
            return 0;
        }
    }

    public async Task<Response> UpsertLeaveAsync(LeaveVM model)
    {
        try
        {
            Leave? leave = (model.LeaveId == 0) ? new Leave() : await _repository.GetAsync(x => x.LeaveId == model.LeaveId && !x.IsDeleted);
            if (model.LeaveId != 0 && leave == null)
                return new Response(false, "Leave not found");
            leave = model.LeaveId == 0 ? _mapper.Map<Leave>(model) : _mapper.Map(model, leave);
            leave.UserId = model.UserId == 0 ? UpsertedBy : model.UserId;
            leave!.UpdatedBy = UpsertedBy;
            if (model.LeaveId == 0)
            {
                if (await _repository.GetAsync(x => x.UserId == leave.UserId && !x.IsDeleted &&
                ((x.FromDate >= leave.FromDate && x.FromDate <= leave.ToDate) || (x.ToDate <= leave.ToDate && x.ToDate >= leave.FromDate) || (x.FromDate <= leave.FromDate && x.ToDate >= leave.ToDate))) != null)
                    return new Response(false, "Leave already exists for this user in the selected date range.");
                leave.CreatedBy = UpsertedBy;
                await _repository.AddAsync(leave);
            }
            else
            {
                leave.UpdatedAt = DateTime.Now;
                _repository.Update(leave);
            }
            await _repository.SaveAsync();
            return new Response(true, string.Format("Leave {0} successfully", model.LeaveId == 0 ? "created" : "updated"), leave);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving Leave.");
            return new Response(false, "An error occurred while saving the Leave.");
        }
    }

    public async Task<List<LeaveVM>> GetLeavesAsync(DateOnly? startDate, DateOnly? endDate, string leaveStatus,int userId)
    {
        try
        {
            List<Leave> leaves = await _repository.GetAllAsync(x => !x.IsDeleted &&
                (userId == 0 || x.UserId == userId) &&
                (startDate == null || x.FromDate >= startDate) &&
                (endDate == null || x.ToDate <= endDate) &&
                (leaveStatus=="All" || (leaveStatus == "Approved" && x.IsApproved == true) || (leaveStatus == "Pending" && x.IsApproved == null) || (leaveStatus == "Rejected" && x.IsApproved == false)),x=>x.User,x=>x.ApprovedByNavigation);
            return leaves.Select(leave => _mapper.Map<LeaveVM>(leave)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting leaves.");
            return new List<LeaveVM>();
        }
    }
}
