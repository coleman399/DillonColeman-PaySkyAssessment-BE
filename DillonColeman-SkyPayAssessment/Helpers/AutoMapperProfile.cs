namespace DillonColeman_SkyPayAssessment.Helpers
{
    // For more information visit: https://docs.automapper.org/en/stable/Configuration.html
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // CreateMap<TSource, TDestination>();
            CreateMap<RegisterUserDto, User>()
                .ForMember(destination => destination.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(destination => destination.RefreshToken, opt => opt.Ignore())
                .ForMember(destination => destination.AccessToken, opt => opt.Ignore());
            CreateMap<User, RegisterUserDto>()
                .ForMember(destination => destination.Password, opt => opt.MapFrom(src => src.PasswordHash));
            CreateMap<GetUserDto, User>();
            CreateMap<User, GetUserDto>();
            CreateMap<RegisterUserDto, GetUserDto>();
            CreateMap<GetUserDto, RegisterUserDto>();
            CreateMap<LoginUserDto, User>()
                .ForMember(destination => destination.PasswordHash, opt => opt.MapFrom(src => src.Password));
            CreateMap<User, LoginUserDto>()
                .ForMember(destination => destination.Password, opt => opt.MapFrom(src => src.PasswordHash));
            CreateMap<User, GetLoggedInUserDto>()
                .ForMember(destination => destination.Token, opt => opt.MapFrom(src => src.AccessToken));
            CreateMap<GetLoggedInUserDto, User>()
                .ForMember(destination => destination.PasswordHash, opt => opt.Ignore());
            CreateMap<UpdateUserDto, User>()
                .ForMember(destination => destination.PasswordHash, opt => opt.Ignore());
            CreateMap<User, UpdateUserDto>();
            CreateMap<DeleteUserDto, User>();
            CreateMap<User, DeleteUserDto>();
            CreateMap<GetUserDto, UpdateUserDto>();
            CreateMap<UpdateUserDto, GetUserDto>();
            CreateMap<GetUserDto, DeleteUserDto>();
            CreateMap<DeleteUserDto, GetUserDto>();
            CreateMap<GetVacancyDto, Vacancy>();
            CreateMap<Vacancy, GetVacancyDto>();
            CreateMap<CreateVacancyDto, Vacancy>();
            CreateMap<Vacancy, CreateVacancyDto>();
            CreateMap<UpdateVacancyDto, Vacancy>();
            CreateMap<Vacancy, UpdateVacancyDto>()
                .ForMember(destination => destination.ExpiresOn, opt => opt.Ignore());
            CreateMap<DeleteVacancyDto, Vacancy>();
            CreateMap<Vacancy, DeleteVacancyDto>();
            CreateMap<GetVacancyDto, UpdateVacancyDto>();
            CreateMap<UpdateVacancyDto, GetVacancyDto>();
            CreateMap<GetVacancyDto, DeleteVacancyDto>();
            CreateMap<DeleteVacancyDto, GetVacancyDto>();
            CreateMap<CreateVacancyDto, GetVacancyDto>();
            CreateMap<GetVacancyDto, CreateVacancyDto>();
        }
    }
}
