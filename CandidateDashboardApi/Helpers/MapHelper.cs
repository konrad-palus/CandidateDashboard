using AutoMapper;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.UserServiceModels;
using Domain.Entities;
using Domain.Entities.CandidateEntities;

namespace CandidateDashboardApi.Helpers
{
    public class MapHelper : Profile
    {
        public MapHelper()
        {
            /*  CreateMap<ApplicationUser, UserDataModel>()
             .ForMember(dest => dest.Employer, opt => opt.MapFrom(src => src.Employer));*/
            CreateMap<UserUpdateModel, ApplicationUser>().ForAllMembers(c => c.Condition((s, d, sm) => sm != null));
            CreateMap<Employer, EmployerDataModel>();



            //registration
            CreateMap<RegistrationModel, Candidate>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.RegistrationEmail))
                .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.RegistrationEmail));

            CreateMap<RegistrationModel, Employer>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.RegistrationEmail))
                .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.RegistrationEmail));

        }
    }
}
