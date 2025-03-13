using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AircraftController : ControllerBase
    {
        private readonly ArrivalFlightService _arrivalFlightService;
        private readonly ILogger<AircraftController> _logger;

        public AircraftController(ArrivalFlightService arrivalFlightService, ILogger<AircraftController> logger)
        {
            _arrivalFlightService = arrivalFlightService;
            _logger = logger;
        }

        [HttpGet("aircraft-arrival")]
        public ActionResult<AircraftArrivalResponse> GetAircraftArrivalInfo(
            [FromQuery] DateTime landingDateTimeToReaport,
            [FromQuery] int occupiedSeats,
            [FromQuery] string baggageToReaport)
        {
            try
            {
                // Проверка входных данных
                if (landingDateTimeToReaport == default)
                {
                    _logger.LogWarning("ArrivalTime is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 300, Message = "ArrivalTime is required" });
                }

                if (occupiedSeats <= 0)
                {
                    _logger.LogWarning("Invalid number of occupied seats.");
                    return BadRequest(new ErrorResponse { ErrorCode = 301, Message = "Invalid number of occupied seats" });
                }

                if (string.IsNullOrEmpty(baggageToReaport))
                {
                    _logger.LogWarning("Invalid baggage capacity.");
                    return BadRequest(new ErrorResponse { ErrorCode = 302, Message = "Invalid baggage capacity" });
                }

                // Получение информации о прилетающем самолете
                var arrivalFlight = _arrivalFlightService.GetArrivalFlightByLandingTime(landingDateTimeToReaport);
                if (arrivalFlight == null)
                {
                    _logger.LogWarning($"Самолет с временем прилета {landingDateTimeToReaport} не найден.");
                    return NotFound(new ErrorResponse { ErrorCode = 303, Message = "Aircraft not found" });
                }

                // Формирование ответа
                var response = new AircraftArrivalResponse
                {
                    LandingDateTimeToReaport = landingDateTimeToReaport,
                    OccupiedSeats = occupiedSeats,
                    BaggageToReaport = baggageToReaport,
                    AircraftStatus = arrivalFlight.HasLanded ? "Прибыл" : "Прибывает"
                };

                _logger.LogInformation($"Информация о прилетающем самолете успешно получена.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о прилетающем самолете.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}