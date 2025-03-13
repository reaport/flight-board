using Microsoft.AspNetCore.Mvc;
using AirportManagement.Services;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityController : ControllerBase
    {
        private readonly CityService _cityService;

        public CityController(CityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet("allowed")]
        public IActionResult GetAllowedCities()
        {
            var cities = _cityService.GetAllowedCities();
            return Ok(cities);
        }
    }
}