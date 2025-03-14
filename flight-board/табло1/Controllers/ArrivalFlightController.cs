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
    public class ArrivalFlightController : ControllerBase
    {
        private readonly ArrivalFlightService _arrivalFlightService;
        private readonly IAircraftService _aircraftService; // Добавляем AircraftService
        private readonly ILogger<ArrivalFlightController> _logger;
        private readonly IConfiguration _config;

        public ArrivalFlightController(
            ArrivalFlightService arrivalFlightService,
            IAircraftService aircraftService, // Внедряем AircraftService
            ILogger<ArrivalFlightController> logger,
            IConfiguration config)
        {
            _arrivalFlightService = arrivalFlightService ?? throw new ArgumentNullException(nameof(arrivalFlightService));
            _aircraftService = aircraftService ?? throw new ArgumentNullException(nameof(aircraftService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ArrivalFlight>> CreateArrivalFlight([FromBody] ArrivalFlightSettings settings)
        {
            if (settings == null)
            {
                _logger.LogWarning("Попытка создания рейса с пустыми настройками.");
                return BadRequest("Настройки рейса не могут быть пустыми.");
            }

            try
            {
                // Создаем рейс на прилет
                var arrivalFlight = _arrivalFlightService.CreateArrivalFlight(settings.DepartureCity, settings.ArrivalTimeOffset);
                _logger.LogInformation($"Создан новый рейс на прилет {arrivalFlight.FlightId} из {settings.DepartureCity}.");

                // Получаем данные о самолете
                var aircraftData = await _aircraftService.GetAircraftDataAsync(arrivalFlight.FlightId);
                arrivalFlight.AircraftData = aircraftData.AircraftId; // Добавляем данные о самолете в рейс

                _logger.LogInformation($"Данные о самолете для рейса {arrivalFlight.FlightId} успешно получены.");

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