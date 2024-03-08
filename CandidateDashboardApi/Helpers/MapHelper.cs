using AutoMapper;
using CandidateDashboardApi.Models.UserServiceModels;
using Domain.Entities;

namespace CandidateDashboardApi.Helpers
{
    public class MapHelper : Profile
    {
        public MapHelper()
        {
            CreateMap<ApplicationUser, UserDataModel>(); 
        }
    }
}
