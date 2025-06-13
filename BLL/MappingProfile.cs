using AutoMapper;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;

namespace BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UpsertUserVM, User>()
        .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId == 0 ? (int?)null : src.ManagerId))
        .ForMember(dest => dest.Department, opt => opt.MapFrom(src => ((Department)src.Department).ToString()));
        CreateMap<User, UpsertUserVM>()
        .ForMember(dest => dest.Department, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Department), src.Department)));
        CreateMap<HolidayVM, Holiday>().ReverseMap();
        CreateMap<UserAttendanceVM, Attendance>().ReverseMap();
    }
}
