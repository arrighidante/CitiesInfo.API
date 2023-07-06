using AutoMapper;
using CitiesInfo.API.Models;
using CitiesInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using System.Transactions;

namespace CitiesInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper
            ) 
        { 
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? 
                throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? 
                throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(
            int cityId)
        {
            if(!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation(
                    $"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

                var transaction = SentrySdk.StartTransaction(
                                "PointsOfInterest",
                                "GetCityPointsOfInterest");
            SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

            try
            {
                var pointsOfInterestForCity = await _cityInfoRepository
                    .GetPointsOfInterestForCityAsync(cityId);

                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));
                
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                              return StatusCode(500, "A problem happened while handling your request.");
            }
            finally { 
                transaction.Finish(); 
            }




            
        }

        [HttpGet("{poiId}", Name ="GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int poiId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var poi = await _cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, poiId);

            if(poi == null) 
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PointOfInterestDto>(poi));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {

            // Validate if city exists
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }


            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);
                        
            await _cityInfoRepository.AddPointOfInterestForCityAsync(
                cityId, finalPointOfInterest );

            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn =
                _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    poiId = createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn);
        }


        //[HttpPut("{poiId}")]
        //public ActionResult UpdatePointOfInterest(
        //  int cityId, int poiId, PointOfInterestForUpdateDto pointOfInterest)
        //{
        //    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var poi = city.PointsOfInterests.FirstOrDefault(p => p.Id == poiId);
        //    if (poi == null)
        //    {
        //        return NotFound();
        //    }

        //    // Find point of interest
        //    var pointOfInterestFromStore = city.PointsOfInterests
        //        .FirstOrDefault(p => p.Id == poiId);
        //    if(pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }

        //    pointOfInterestFromStore.Name = pointOfInterest.Name;
        //    pointOfInterestFromStore .Description = pointOfInterest.Description;

        //    return NoContent();

        //}

        //[HttpPatch("{poiId}")]
        //public ActionResult PartiallyUpdatePointOfInterest(
        //    // JsonPatchDocument is a list of operations we want to apply to our PointOfInterest
        //int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        //{
        //    // Get city
        //    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    // Find point of interest
        //    var pointOfInterestFromStore = city.PointsOfInterests
        //        .FirstOrDefault(p => p.Id == poiId);
        //    if (pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }


        //    var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
        //    {
        //        Name = pointOfInterestFromStore.Name,
        //        Description = pointOfInterestFromStore.Description,
        //    };

        //    patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    if (!TryValidateModel(pointOfInterestToPatch))
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        //    pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

        //    return NoContent();


        //}

        //[HttpDelete("{poiId}")]
        //public ActionResult DeletePointOfInterest(
        //  int cityId, int poiId)
        //{
        //    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterestFromStore = city.PointsOfInterests.FirstOrDefault(p => p.Id == poiId);
        //    if (pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }


        //    city.PointsOfInterests.Remove(pointOfInterestFromStore);
        //    _mailService.Send(
        //        "Point of interest deleted.",
        //        $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");

        //    return NoContent();
        //}


    }
}

    

