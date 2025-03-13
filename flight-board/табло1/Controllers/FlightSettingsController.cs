using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightSettingsController : ControllerBase
    {
        private readonly FlightSettings _settings;
        private readonly ILogger<FlightSettingsController> _logger;
        private readonly FlightService _flightService;
        private readonly ILoggerFactory _loggerFactory;

        public FlightSettingsController(
            FlightSettings settings,
            ILogger<FlightSettingsController> logger,
            FlightService flightService,
            ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _logger = logger;
            _flightService = flightService;
            _loggerFactory = loggerFactory;
        }

        [HttpPost("save")]
        public IActionResult SaveSettings([FromBody] FlightSettingsDto newSettings)
        {
            if (newSettings == null)
            {
                _logger.LogWarning("Настройки не могут быть пустыми.");
                return BadRequest("Настройки не могут быть пустыми.");
            }

            // Логируем полученные настройки
            _logger.LogInformation("Получены настройки: {@Settings}", newSettings);

            // Обновляем настройки
            _settings.PurchaseToRegistrationMinutes = newSettings.PurchaseToRegistrationMinutes;
            _settings.RegistrationToBoardingMinutes = newSettings.RegistrationToBoardingMinutes;
            _settings.BoardingToEndBoardingMinutes = newSettings.BoardingToEndBoardingMinutes;
            _settings.EndBoardingToDepartureMinutes = newSettings.EndBoardingToDepartureMinutes;

            // Логируем обновленные настройки
            _logger.LogInformation("Настройки обновлены: {@Settings}", _settings);

            // Создаем новый рейс с выбранным городом
            var departureFlightLogger = _loggerFactory.CreateLogger<DepartureFlightGenerator>();
            var flight = _flightService.CreateFlight(newSettings.Destination, departureFlightLogger, _settings);

            // Логируем создание рейса
            _logger.LogInformation("Рейс создан: {@Flight}", flight);

            return Ok(new { success = true });
        }

        [HttpGet("get")]
        public ActionResult<FlightSettings> GetSettings()
        {
            _logger.LogInformation("Запрошены текущие настройки.");
            return Ok(_settings);
        }

        [HttpGet("flights")]
        public ActionResult<List<DepartureFlightGenerator>> GetFlights()
        {
            var flights = _flightService.GetAllFlights();
            _logger.LogInformation("Запрошен список рейсов. Количество рейсов: {Count}", flights.Count);
            return Ok(flights);
        }
    }
}