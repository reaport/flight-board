using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AircraftDepartureController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly ILogger<AircraftDepartureController> _logger;

        public AircraftDepartureController(FlightService flightService, ILogger<AircraftDepartureController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        [HttpGet("aircraft-departure")]
        public ActionResult<AircraftDepartureResponse> GetAircraftDepartureInfo(
            [FromQuery] string flightId,
            [FromQuery] DateTime boardingStartTime,
            [FromQuery] DateTime boardingEndTime,
            [FromQuery] List<SeatAvailability> boughtSeats) // Изменено на SeatAvailability
        {
            try
            {
                // Проверка входных данных
                if (string.IsNullOrEmpty(flightId))
                {
                    _logger.LogWarning("FlightId is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 400, Message = "FlightId is required" });
                }

                if (boardingStartTime == default || boardingEndTime == default || boardingStartTime >= boardingEndTime)
                {
                    _logger.LogWarning("Invalid boarding times provided.");
                    return BadRequest(new ErrorResponse { ErrorCode = 401, Message = "Invalid boarding times provided" });
                }

                if (boughtSeats == null || boughtSeats.Count == 0)
                {
                    _logger.LogWarning("Invalid bought seats format.");
                    return BadRequest(new ErrorResponse { ErrorCode = 402, Message = "Invalid bought seats format" });
                }

                // Получение информации о рейсе
                var flight = _flightService.GetFlight(flightId); // Теперь flightId передается как строка
                if (flight == null)
                {
                    _logger.LogWarning($"Рейс {flightId} не найден.");
                    return NotFound(new ErrorResponse { ErrorCode = 403, Message = "Flight not found" });
                }

                // Формирование ответа
                var response = new AircraftDepartureResponse
                {
                    FlightId = flightId,
                    BoardingStartTime = boardingStartTime,
                    BoardingEndTime = boardingEndTime,
                };

                _logger.LogInformation($"Информация о вылетающем самолете успешно получена.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о вылетающем самолете.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}