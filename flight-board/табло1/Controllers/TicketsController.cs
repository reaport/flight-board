using AirportManagement.Models;
using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly FlightService _flightService;
        private readonly IAircraftService _aircraftService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            FlightService flightService,
            IAircraftService aircraftService,
            ILogger<TicketsController> logger)
        {
            _flightService = flightService;
            _aircraftService = aircraftService;
            _logger = logger;
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<TicketPurchaseResponse>>> GetAvailableFlights()
        {
            try
            {
                // Получение списка всех рейсов
                var flights = _flightService.GetAllFlights();

                var result = new List<TicketPurchaseResponse>();

                foreach (var flight in flights)
                {
                    // Получение данных о самолете
                    var aircraftData = await _aircraftService.GetAircraftDataAsync(flight.AircraftId);

                    // Группировка мест по классам
                    var availableSeatsByClass = aircraftData.Seats
                        .GroupBy(s => s.SeatClass)
                        .Select(g => new AvailableSeatsInfo // Используем AvailableSeatsInfo
                        {
                            SeatClass = g.Key,
                            SeatCount = g.Count()
                        })
                        .ToList();

                    // Формирование ответа для каждого рейса
                    var flightInfo = new TicketPurchaseResponse
                    {
                        FlightId = flight.FlightId,
                        CityFrom = flight.CityFrom,
                        CityTo = flight.CityTo,
                        TakeoffDateTime = flight.DepartureTime,
                        RegistrationStartTime = flight.RegistrationStartTime, // Добавляем время начала регистрации
                        AvailableSeats = availableSeatsByClass
                    };

                    result.Add(flightInfo);
                }

                _logger.LogInformation("Список доступных рейсов успешно получен.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка доступных рейсов.");
                return StatusCode(500, new ErrorResponse { ErrorCode = 500, Message = "InternalServerError" });
            }
        }
    }
}