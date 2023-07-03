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
        private readonly CitiesDataStore _citiesDataStore;
        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            CitiesDataStore citiesDataStore
            ) 
        { 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(_mailService));    
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(_citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
                var transaction = SentrySdk.StartTransaction(
                                "PointsOfInterest",
                                "GetCityPointsOfInterest");
            SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

            try
            {
                //throw new Exception("Exception sample.");
             
                var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }
                 
                return Ok(city.PointOfInterests);
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
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int poiId)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var poi = city.PointOfInterests.FirstOrDefault(p => p.Id == poiId);
            if (poi == null)
            {
                return NotFound();
            }

            return Ok(poi);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {

           

            // Validate if city exists
            var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // just a temp solution
            var maxPointOfInterestId = _citiesDataStore.Cities.SelectMany(
                               c => c.PointOfInterests).Max(p => p.Id);

            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description,
            };

            city.PointOfInterests.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    poiId= finalPointOfInterest.Id
                },
                finalPointOfInterest);
        }


        [HttpPut("{poiId}")]
        public ActionResult UpdatePointOfInterest(
          int cityId, int poiId, PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var poi = city.PointOfInterests.FirstOrDefault(p => p.Id == poiId);
            if (poi == null)
            {
                return NotFound();
            }

            // Find point of interest
            var pointOfInterestFromStore = city.PointOfInterests
                .FirstOrDefault(p => p.Id == poiId);
            if(pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore .Description = pointOfInterest.Description;

            return NoContent();

        }

        [HttpPatch("{poiId}")]
        public ActionResult PartiallyUpdatePointOfInterest(
            // JsonPatchDocument is a list of operations we want to apply to our PointOfInterest
        int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            // Get city
            var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            // Find point of interest
            var pointOfInterestFromStore = city.PointOfInterests
                .FirstOrDefault(p => p.Id == poiId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }


            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description,
            };

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }
            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();


        }

        [HttpDelete("{poiId}")]
        public ActionResult DeletePointOfInterest(
          int cityId, int poiId)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointOfInterests.FirstOrDefault(p => p.Id == poiId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }


            city.PointOfInterests.Remove(pointOfInterestFromStore);
            _mailService.Send(
                "Point of interest deleted.",
                $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");

            return NoContent();
        }


    }
}

    

