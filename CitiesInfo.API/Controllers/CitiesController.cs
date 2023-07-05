﻿using AutoMapper;
using CitiesInfo.API.Models;
using CitiesInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CitiesInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {

        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        // Constructor
        public CitiesController(ICityInfoRepository cityInfoRepository,
            IMapper mapper) 
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Endpoints

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));

            /* // SOLUTION WITHOUT AUTOMAPPER: Need to map from city entity to CityWithoutPointsOfInterestDTO

            var results = new List<CityWithoutPointsOfInterestDto>();
            foreach (var cityEntity in cityEntities)
            {
                results.Add(new CityWithoutPointsOfInterestDto
                { 
                    Id = cityEntity.Id,
                    Description = cityEntity.Description,
                    Name = cityEntity.Name,
                });
            }

            return Ok(results);
            */
        }

        [HttpGet("{id}")]
        // CityDto it´s not exactly what the endpoint is going to return:
        //          public async Task<ActionResult<CityDto>> GetCity
        // so, instead, we'll use  Task<IActionResult> which is a more generic approach
        public async Task<IActionResult> GetCity(
            int id, 
            bool includePointsOfInterest = false)
        {

            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                var resultado = _mapper.Map<CityDto>(city);
                return Ok(resultado);
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));

        }


    }
}
