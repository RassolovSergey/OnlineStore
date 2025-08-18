using AutoMapper;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.DTOs.Auth;

namespace OnlineStore.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity -> DTO для /auth/me
            CreateMap<User, UserProfileDto>();

            // DTO -> Entity (для регистрации) — если когда-нибудь понадобится
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
