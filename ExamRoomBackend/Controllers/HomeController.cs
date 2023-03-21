using ExamRoomBackend.Models;
using ExamRoomBackend.Models.DTO;
using ExamRoomBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamRoomBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILocationService _locationService;

        public HomeController(IEmailService emailService, ILocationService locationService)
        {
            _emailService = emailService;
            _locationService = locationService;
        }

        [HttpGet("verifyByEmail")]
        public IActionResult GetVerificationEmail()
        {
            try
            {
                // credentials to pass to the email HTML template
                UserEmailOptions options = new UserEmailOptions
                {
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                    new KeyValuePair<string, string>("{{ UserName }}", "John"),
                    new KeyValuePair<string, string>("{{ Code }}", _emailService.GenerateVerificationCode())
                    }
                };
                // send the email 
                _emailService.SendVerificationEmail(options);
                return Ok();
            }catch(Exception ex)
            {
                return BadRequest($"Error occured: {ex.Message}");
            }
        }

        [HttpPost("verifyBySMS")]
        public IActionResult GetVerificationSMS()
        {
            try
            {
                _emailService.SendVerificationSMS("+2348080978412", _emailService.GenerateVerificationCode(), "John");
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest($"Error occured: {ex.Message}");
            }
        }

        [HttpGet("getCities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _locationService.GetCities();
            if (cities == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No cities");
            }
            return StatusCode(StatusCodes.Status200OK, cities);
        }

        [HttpGet("getCountryStates")]
        public async Task<IActionResult> GetCountryStates(int countryId)
        {
            var countryStates = await _locationService.GetCountryStates(countryId);
            if (countryStates == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No states available");
            }
            return StatusCode(StatusCodes.Status200OK, countryStates);
        }


        [HttpGet("getCountries")]
        public async Task<IActionResult> GetCountries(string? countryName)
        {
            var countries = await _locationService.GetCountries(countryName);
            if (countries == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No countries");
            }
            return StatusCode(StatusCodes.Status200OK, countries);
        }

        [HttpGet("getGenders")]
        public async Task<IActionResult> GetGenders()
        {
            var genders = await _locationService.GetGenders();
            if (genders == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No genders");
            }
            return StatusCode(StatusCodes.Status200OK, genders);
        }
        
        [HttpGet("getSates")]
        public async Task<IActionResult> GetStates()
        {
            var states = await _locationService.GetStates();
            if (states == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No states");
            }
            return StatusCode(StatusCodes.Status200OK, states);
        }
        
        [HttpGet("getStateCities")]
        public async Task<IActionResult> GetStateCities(int id, bool includeCities)
        {
            var cities = await _locationService.GetStateCities(id, includeCities);
            if (cities == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No cities");
            }
            return StatusCode(StatusCodes.Status200OK, cities);
        }

    }
}
