using AutoMapper;
using Entity.Models;
using Entity.ViewModels;

namespace BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UpsertUserVM, User>()
        .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId == 0 ? (int?)null : src.ManagerId));
        CreateMap<User, UpsertUserVM>();
        CreateMap<HolidayVM, Holiday>().ReverseMap();
        CreateMap<UserAttendanceVM, Attendance>().ReverseMap();
    }
}
