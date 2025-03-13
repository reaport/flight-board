using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly IAircraftService _aircraftService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(
            FlightService flightService,
            IAircraftService aircraftService,
            ILogger<TicketController> logger)
        {
            _flightService = flightService;
            _aircraftService = aircraftService;
            _logger = logger;
        }

        [HttpGet("tickets")]
        public async Task<ActionResult<TicketPurchaseResponse>> GetTicketPurchaseInfo(
            [FromQuery] string flightId,
            [FromQuery] string seatClass,
            [FromQuery] string direction) // direction = CityTo
        {
            try
            {
                // Проверка входных данных
                if (string.IsNullOrEmpty(flightId))
                {
                    _logger.LogWarning("FlightId is required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 200, Message = "FlightId is required" });
                }

                if (string.IsNullOrEmpty(seatClass) || string.IsNullOrEmpty(direction))
                {
                    _logger.LogWarning("SeatClass and Direction are required.");
                    return BadRequest(new ErrorResponse { ErrorCode = 202, Message = "SeatClass and Direction are required" });
                }

                // Получение информации о рейсе
                var flight = _flightService.GetFlight(flightId);
                if (flight == null)
                {
                    _logger.LogWarning($"Рейс {flightId} не найден.");
                    return NotFound(new ErrorResponse { ErrorCode = 203, Message = "Flight not found" });
                }

                // Проверка, что рейс соответствует запросу
                if (flight.CityTo != direction)
                {
                    _logger.LogWarning($"Рейс {flightId} не соответствует указанному направлению.");
                    return BadRequest(new ErrorResponse { ErrorCode = 204, Message = "Flight does not match the specified direction" });
                }

                // Получение данных о самолете
                var aircraftData = await _aircraftService.GetAircraftDataAsync(flight.AircraftId);

                // Фильтрация доступных мест по классу
                var availableSeats = aircraftData.Seats
                    .Where(s => s.SeatClass == seatClass)
                    .ToList();

                // Формирование ответа
                var response = new TicketPurchaseResponse
                {
                    FlightId = flightId,
                    SeatClass = seatClass,
                    Direction = direction,
                    DepartureTime = flight.DepartureTime,
                    AvailableSeats = availableSeats
                };

                _logger.LogInformation($"Информация о покупке билетов для рейса {flightId} успешно получена.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о покупке билетов.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}