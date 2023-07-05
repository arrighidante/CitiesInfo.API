using AutoMapper;

namespace CitiesInfo.API.Profiles
{
    public class PointOfInterestProfile : Profile
    {
        public PointOfInterestProfile() 
        {
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
        }
    }
}
