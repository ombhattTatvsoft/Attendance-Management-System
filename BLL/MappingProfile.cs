using AutoMapper;
using Entity.Enums;
using Entity.Models;
using Entity.ViewModels;

namespace BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UpsertUserVM>()
        .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => (int)src.ManagerId))
        .ForMember(dest => dest.Department, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Department), src.Department)));
        CreateMap<UpsertUserVM, User>();
    }
}
