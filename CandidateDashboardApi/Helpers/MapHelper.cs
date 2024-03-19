
using AutoMapper;
using CandidateDashboardApi.Models.UserServiceModels;
using Domain.Entities;
using Domain.Entities.CandidateEntities;

namespace CandidateDashboardApi.Helpers
{
    public class MapHelper : Profile
    {
        public MapHelper()
        {
            CreateMap<ApplicationUser, UserDataModel>();

            CreateMap<Employer, UserDataModel>()
                .IncludeBase<ApplicationUser, UserDataModel>();

            CreateMap<RegistrationModel, Candidate>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.RegistrationEmail))
                .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.RegistrationEmail));

            CreateMap<RegistrationModel, Employer>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.RegistrationEmail))
                .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.RegistrationEmail));

            CreateMap<UserDataModel, Employer>();
        }
    }
}