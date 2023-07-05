﻿using CitiesInfo.API.Entities;

namespace CitiesInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        
        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);

        Task<bool> CityExistsAsync(int cityId);

        Task<IEnumerable<PointOfInterest?>> GetPointsOfInterestForCityAsync(
            int cityId);

        Task<PointOfInterest?> GetPointOfInterestForCityAsync(
           int cityId, int pointOfInterestId);


    }
}