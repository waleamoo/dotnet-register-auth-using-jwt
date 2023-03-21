using ExamRoomBackend.Models.DTO;

namespace ExamRoomBackend.Services
{
    public interface ILocationService
    {
        Task<List<CityReadDto>> GetCities();
        Task<List<StateReadDto>> GetStates();
        Task<List<GenderReadDto>> GetGenders();
        Task<List<CityReadDto>> GetStateCities(int id, bool includeCities);
        Task<List<CountryReadDto>> GetCountries(string countryName);
        Task<List<StateReadDto>> GetCountryStates(int countryId);
    }
}
