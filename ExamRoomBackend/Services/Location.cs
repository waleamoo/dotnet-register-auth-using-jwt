using AutoMapper;
using ExamRoomBackend.Models.Data;
using ExamRoomBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace ExamRoomBackend.Services
{
    public class Location : ILocationService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Location(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<List<CityReadDto>> GetCities()
        {
            try
            {
                var cities = await _context.Cities.ToListAsync();
                var citiesToReturn = _mapper.Map<List<CityReadDto>>(cities);
                return citiesToReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CountryReadDto>> GetCountries(string? countryName)
        {
            try
            {
                if (!string.IsNullOrEmpty(countryName))
                {
                    // filter countries by name
                    var countriesFilter = await _context.Countries.Where(x => x.CountryName.Contains(countryName)).ToListAsync();
                    return _mapper.Map<List<CountryReadDto>>(countriesFilter);
                }

                var countriesList = await _context.Countries.ToListAsync();
                return _mapper.Map<List<CountryReadDto>>(countriesList);

            }
            catch (Exception ex)
            {
                return null;
            }      
        }

        public async Task<List<StateReadDto>> GetCountryStates(int countryId)
        {
            try
            {
                var states = await _context.States.Where(x => x.CountryId == countryId).ToListAsync();
                return _mapper.Map<List<StateReadDto>>(states);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<GenderReadDto>> GetGenders()
        {
            try
            {
                var genders = await _context.Genders.ToListAsync();
                var gendersToReturn = _mapper.Map<List<GenderReadDto>>(genders);
                return gendersToReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<StateReadDto>> GetStates()
        {
            try
            {
                var states = await _context.States.ToListAsync();
                var statesToReturn = _mapper.Map<List<StateReadDto>>(states);
                return statesToReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CityReadDto>> GetStateCities(int id, bool includeCities)
        {
            try
            {
                if (includeCities)
                {
                    var cities = await _context.Cities.Where(x => x.StateId == id).ToListAsync();
                    var citiesToReturn = _mapper.Map<List<CityReadDto>>(cities);
                    return citiesToReturn;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
