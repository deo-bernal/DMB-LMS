using AutoMapper;
using Dmb.Lms.Data.Entities;
using Dmb.Lms.Model.Dtos.Auth;

namespace Dmb.Lms.Data.Mapper;

public class LmsMappingProfile : Profile
{
    public LmsMappingProfile()
    {
        CreateMap<LmsUser, LoggedInUserDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id));
    }
}
