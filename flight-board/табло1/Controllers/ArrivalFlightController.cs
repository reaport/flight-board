using AirportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AirportManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArrivalFlightController : ControllerBase
    {
        private readonly ArrivalFlightService _arrivalFlightService;
        private readonly ILogger<ArrivalFlightController> _logger; // Используем ILogger<ArrivalFlightController>
        private readonly IConfiguration _config;

        public ArrivalFlightController(
            ArrivalFlightService arrivalFlightService,
            ILogger<ArrivalFlightController> logger, // Исправляем тип логгера
            IConfiguration config)
        {
            _arrivalFlightService = arrivalFlightService ?? throw new ArgumentNullException(nameof(arrivalFlightService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpPost("create")]
        public ActionResult<ArrivalFlight> CreateArrivalFlight([FromBody] ArrivalFlightSettings settings)
        {
            if (settings == null)
            {
                _logger.LogWarning("Попытка создания рейса с пустыми настройками.");
                return BadRequest("Настройки рейса не могут быть пустыми.");
            }

            try
            {
                var arrivalFlight = _arrivalFlightService.CreateArrivalFlight(settings.DepartureCity, settings.ArrivalTimeOffset);
                _logger.LogInformation($"Создан новый рейс на прилет {arrivalFlight.FlightId} из {settings.DepartureCity}.");
                return Ok(arrivalFlight);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Ошибка при создании рейса на прилет: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании рейса на прилет.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{flightId}")]
        public ActionResult<ArrivalFlight> GetArrivalFlightInfo(string flightId)
        {
            if (string.IsNullOrEmpty(flightId))
            {
                _logger.LogWarning("Попытка запроса информации о рейсе с пустым flightId.");
                return BadRequest("Идентификатор рейса не может быть пустым.");
            }

            try
            {
                var arrivalFlight = _arrivalFlightService.GetArrivalFlight(flightId);
                if (arrivalFlight == null)
                {
                    _logger.LogWarning($"Рейс на прилет {flightId} не найден.");
                    return NotFound();
                }

                _logger.LogInformation($"Запрос информации о рейсе на прилет {flightId}.");
                return Ok(arrivalFlight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о рейсе на прилет.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all")]
        public ActionResult<List<ArrivalFlight>> GetAllArrivalFlights()
        {
            try
            {
                var flights = _arrivalFlightService.GetAllArrivalFlights();
                _logger.LogInformation("Запрошен список всех рейсов на прилет. Количество рейсов: {Count}", flights.Count);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка рейсов на прилет.");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ArrivalFlightSettings
    {
        public string DepartureCity { get; set; }
        public int ArrivalTimeOffset { get; set; }
    }
}