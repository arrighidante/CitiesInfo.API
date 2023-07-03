using CitiesInfo.API.Models;

namespace CitiesInfo.API
{
    public class CitiesDataStore
    {
        public List<CityDto> Cities { get; set; }

        //public static CitiesDataStore Current { get; } = new CitiesDataStore();

        public CitiesDataStore() {

            Cities = new List<CityDto>()
            {
                new CityDto()
                {
                    Id = 1,
                    Name = "New York City",
                    Description ="The one with that big park.",
                    PointOfInterests = new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 1,
                            Name = "Central Park",
                            Description = "The most visited urban park in the United States"
                        },
                        new PointOfInterestDto()
                        {
                            Id = 2,
                            Name = "Empire State Building",
                            Description = "A 102-story skyscraper located in Midtown Manhattan"
                        }
                    }
                },
                new CityDto()
                {
                    Id = 2,
                    Name = "Rosario",
                    Description ="The one with the Argentinian flag monument.",
                    PointOfInterests= new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 1,
                            Name = "Monumento",
                            Description = "The most visited Monumento close to the Parana river"
                        },
                        new PointOfInterestDto()
                        {
                            Id = 2,
                            Name = "Infinita Panaderia",
                            Description = "The Best sour flour bakery in Rosario"
                        }
                    }

                },
                new CityDto()
                {
                    Id = 3,
                    Name = "Paris",
                    Description ="The one with the Eiffel tower.",
                    PointOfInterests = new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 1,
                            Name = "Eiffel Tower",
                            Description = "A wrought iron lattice tower on the Champ de Mars"
                        },
                        new PointOfInterestDto()
                        {
                            Id = 2,
                            Name = "The Louvre",
                            Description = "The world's largest museum."
                        }
                    }
        }
            };
        }

      
    }
}
